using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Constants;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// Handles registration, sign-in, and sign-out. Every new registrant receives
/// the Guest role, and each of the three actions writes an activity log entry.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IActivityLogRepository _activityLog;

    /// <summary>
    /// Initialises a new instance of the <see cref="AccountController"/> class.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="signInManager">The Identity sign-in manager.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IActivityLogRepository activityLog)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _activityLog = activityLog;
    }

    /// <summary>
    /// Renders the registration form.
    /// </summary>
    /// <returns>The registration page.</returns>
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    /// <summary>
    /// Creates the account, assigns the Guest role, and signs the user in.
    /// </summary>
    /// <param name="model">The submitted registration form.</param>
    /// <returns>A redirect to the landing page on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName,
            LastName = model.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // Every registrant starts as a Guest and applies to become a Host later.
        await _userManager.AddToRoleAsync(user, Roles.Guest);
        await _signInManager.SignInAsync(user, isPersistent: false);
        await _activityLog.LogAsync(user.Id, "UserRegistered", ActivityType.Login, "Guest role assigned on registration.");

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    /// <summary>
    /// Renders the sign-in form.
    /// </summary>
    /// <param name="returnUrl">The URL to return to after signing in.</param>
    /// <returns>The sign-in page.</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    /// <summary>
    /// Verifies the credentials and routes the user to their role's dashboard.
    /// </summary>
    /// <param name="model">The submitted sign-in form.</param>
    /// <returns>A redirect on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid sign-in attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                result.IsLockedOut
                    ? "This account is temporarily locked. Try again shortly."
                    : "Invalid sign-in attempt.");

            return View(model);
        }

        await _activityLog.LogAsync(user.Id, "UserLoggedIn", ActivityType.Login, $"{user.Email} signed in.");

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return await RedirectToDashboardAsync(user);
    }

    /// <summary>
    /// Signs the user out and records the event.
    /// </summary>
    /// <returns>A redirect to the landing page.</returns>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);

        if (userId is not null)
            await _activityLog.LogAsync(userId, "UserLoggedOut", ActivityType.Logout);

        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    /// <summary>
    /// Renders the page shown when a user reaches a route their role cannot access.
    /// </summary>
    /// <returns>The access-denied page.</returns>
    [HttpGet]
    public IActionResult AccessDenied() => View();

    /// <summary>
    /// Chooses the landing destination that matches the user's role.
    /// </summary>
    /// <param name="user">The signed-in user.</param>
    /// <returns>The redirect for their role.</returns>
    private async Task<IActionResult> RedirectToDashboardAsync(ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin) ||
            await _userManager.IsInRoleAsync(user, Roles.Admin))
            return RedirectToAction("Dashboard", "Admin");

        if (await _userManager.IsInRoleAsync(user, Roles.Host))
            return RedirectToAction("Dashboard", "Host");

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
