using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Constants;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Services;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// The "Become a Host" flow. A Guest applies here for their first listing; an
/// existing Host uses the same form to apply for an additional property.
/// </summary>
[Authorize(Roles = $"{Roles.Guest},{Roles.Host}")]
public class HostApplicationsController : Controller
{
    private readonly IHostApplicationRepository _applications;
    private readonly IPropertyRepository _properties;
    private readonly IActivityLogRepository _activityLog;
    private readonly INotificationRepository _notifications;
    private readonly IFileStorageService _files;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="HostApplicationsController"/> class.
    /// </summary>
    /// <param name="applications">The application repository.</param>
    /// <param name="properties">The listing repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="files">The upload storage service.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public HostApplicationsController(
        IHostApplicationRepository applications,
        IPropertyRepository properties,
        IActivityLogRepository activityLog,
        INotificationRepository notifications,
        IFileStorageService files,
        UserManager<ApplicationUser> userManager)
    {
        _applications = applications;
        _properties = properties;
        _activityLog = activityLog;
        _notifications = notifications;
        _files = files;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the application form.
    /// </summary>
    /// <returns>The application page.</returns>
    [HttpGet]
    public IActionResult Apply() => View(new HostApplicationViewModel());

    /// <summary>
    /// Validates and submits an application. The listing is created inactive and
    /// stays that way until an administrator approves the application.
    /// </summary>
    /// <param name="model">The submitted application form.</param>
    /// <returns>A redirect to the status page on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(HostApplicationViewModel model)
    {
        // An application is incomplete without at least one photograph.
        var usableImages = model.Images.Where(_files.IsValid).ToList();

        if (usableImages.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.Images),
                "At least one property photo is required. Accepted formats: JPG, PNG, WEBP, or GIF up to 5 MB.");
        }

        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User)!;
        var isExistingHost = User.IsInRole(Roles.Host);

        var property = new HostProperty
        {
            Title = model.Title,
            Description = model.Description,
            PricePerNight = model.PricePerNight,
            CleaningFee = model.CleaningFee,
            Address = model.Address,
            City = model.City,
            PropertyType = model.PropertyType,
            MaxGuests = model.MaxGuests,
            Bedrooms = model.Bedrooms,
            Beds = model.Beds,
            Bathrooms = model.Bathrooms,
            HostId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // The listing stays unpublished until an administrator approves it.
        property.Deactivate();

        var isFirst = true;
        foreach (var image in usableImages)
        {
            property.Images.Add(new PropertyImage
            {
                ImageUrl = await _files.SaveAsync(image, "properties"),
                IsCover = isFirst
            });

            isFirst = false;
        }

        var propertyId = await _properties.CreateAsync(property);

        var application = new HostApplication
        {
            ApplicationUserId = userId,
            PropertyId = propertyId,
            Status = ApplicationStatus.Pending,
            IsAdditionalProperty = isExistingHost,
            AppliedAt = DateTime.UtcNow
        };

        await _applications.CreateAsync(application);

        var action = isExistingHost ? "AdditionalPropertyApplicationSubmitted" : "HostApplicationSubmitted";
        await _activityLog.LogAsync(userId, action, ActivityType.ApplicationSubmitted, model.Title);

        await _notifications.CreateAsync(
            userId,
            isExistingHost ? "Additional Property Application Submitted" : "Host Application Submitted",
            $"Your application for '{model.Title}' has been received and is under review.",
            isExistingHost ? NotificationType.AdditionalPropertyApplication : NotificationType.System,
            "/HostApplications/Status");

        TempData["Success"] = "Your application has been submitted and is awaiting review.";
        return RedirectToAction(nameof(Status));
    }

    /// <summary>
    /// Shows the applicant the state of every application they have submitted.
    /// </summary>
    /// <returns>The status page.</returns>
    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _applications.GetByUserAsync(userId));
    }
}
