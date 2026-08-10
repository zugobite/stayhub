using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Services;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// Lets any signed-in user maintain the profile other people see: their name,
/// introduction, home town, and photograph.
/// </summary>
/// <remarks>
/// Open to every role rather than hosts alone. A guest's name and photograph
/// appear beside their reviews and in message threads, so the details are not
/// host-specific.
/// </remarks>
[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _files;
    private readonly IActivityLogRepository _activityLog;

    /// <summary>
    /// Initialises a new instance of the <see cref="ProfileController"/> class.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="files">The upload storage service.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    public ProfileController(
        UserManager<ApplicationUser> userManager,
        IFileStorageService files,
        IActivityLogRepository activityLog)
    {
        _userManager = userManager;
        _files = files;
        _activityLog = activityLog;
    }

    /// <summary>
    /// Renders the profile editor for the signed-in user.
    /// </summary>
    /// <returns>The profile page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Challenge();

        return View(await BuildAsync(user));
    }

    /// <summary>
    /// Saves the signed-in user's profile.
    /// </summary>
    /// <param name="model">The submitted profile.</param>
    /// <returns>The profile page, with a confirmation on success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user is null)
            return Challenge();

        // A rejected upload must not fail silently: say so rather than saving the
        // rest of the form and leaving the old photo in place unexplained.
        if (model.Photo is not null && !_files.IsValid(model.Photo))
        {
            ModelState.AddModelError(nameof(model.Photo),
                "That photo could not be used. Use a JPG, PNG, WEBP or GIF under 5 MB.");
        }

        if (!ModelState.IsValid)
            return View(await BuildAsync(user, model));

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();

        if (model.Photo is not null)
            user.ProfilePictureUrl = await _files.SaveAsync(model.Photo, "avatars");
        else if (model.RemovePhoto)
            user.ProfilePictureUrl = null;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(await BuildAsync(user, model));
        }

        await _activityLog.LogAsync(
            user.Id,
            "ProfileUpdated",
            ActivityType.ProfileUpdated,
            $"{user.Email} updated their profile.");

        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Builds the view model, carrying the read-only account facts alongside any
    /// values the user has just typed.
    /// </summary>
    private async Task<ProfileViewModel> BuildAsync(ApplicationUser user, ProfileViewModel? submitted = null)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new ProfileViewModel
        {
            FirstName = submitted?.FirstName ?? user.FirstName,
            LastName = submitted?.LastName ?? user.LastName,
            Bio = submitted?.Bio ?? user.Bio,
            Location = submitted?.Location ?? user.Location,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList(),
            MemberSince = user.CreatedAt
        };
    }
}
