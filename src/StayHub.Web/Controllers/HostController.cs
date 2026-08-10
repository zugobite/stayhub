using System.Text.Json;
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
/// The Host Dashboard. Every route is Host-only, so a Guest who has not been
/// approved cannot reach any of it.
/// </summary>
[Authorize(Roles = Roles.Host)]
public class HostController : Controller
{
    private readonly IPropertyRepository _properties;
    private readonly IBookingRepository _bookings;
    private readonly IReviewRepository _reviews;
    private readonly ICheckInRepository _checkIns;
    private readonly INotificationRepository _notifications;
    private readonly IActivityLogRepository _activityLog;
    private readonly IFileStorageService _files;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="HostController"/> class.
    /// </summary>
    /// <param name="properties">The listing repository.</param>
    /// <param name="bookings">The booking repository.</param>
    /// <param name="reviews">The review repository.</param>
    /// <param name="checkIns">The check-in repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="files">The upload storage service.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public HostController(
        IPropertyRepository properties,
        IBookingRepository bookings,
        IReviewRepository reviews,
        ICheckInRepository checkIns,
        INotificationRepository notifications,
        IActivityLogRepository activityLog,
        IFileStorageService files,
        UserManager<ApplicationUser> userManager)
    {
        _properties = properties;
        _bookings = bookings;
        _reviews = reviews;
        _checkIns = checkIns;
        _notifications = notifications;
        _activityLog = activityLog;
        _files = files;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the host dashboard: stat cards, quick actions, and listing cards.
    /// </summary>
    /// <returns>The dashboard page.</returns>
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var user = (await _userManager.GetUserAsync(User))!;
        var properties = (await _properties.GetByHostAsync(user.Id)).ToList();
        var bookings = (await _bookings.GetByHostAsync(user.Id)).ToList();
        var reviews = (await _reviews.GetByHostAsync(user.Id)).ToList();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var earning = new[] { BookingStatus.Paid, BookingStatus.CheckedIn, BookingStatus.Completed };

        var model = new HostDashboardViewModel
        {
            Host = user,
            Properties = properties,
            TotalProperties = properties.Count,
            ActiveProperties = properties.Count(p => p.IsActive),
            TotalBookings = bookings.Count,
            BookingsThisMonth = bookings.Count(b => b.CreatedAt >= monthStart),
            TotalEarnings = bookings.Where(b => earning.Contains(b.Status)).Sum(b => b.TotalPrice),
            EarningsThisMonth = bookings
                .Where(b => earning.Contains(b.Status) && b.CreatedAt >= monthStart)
                .Sum(b => b.TotalPrice),
            ReviewCount = reviews.Count,
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating),
            PendingRequests = bookings.Count(b => b.Status == BookingStatus.Pending)
        };

        return View(model);
    }

    /// <summary>
    /// Lists the host's properties.
    /// </summary>
    /// <returns>The properties page.</returns>
    [HttpGet]
    public async Task<IActionResult> Properties()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _properties.GetByHostAsync(userId));
    }

    /// <summary>
    /// Renders the listing editor for one of the host's properties.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <returns>The editor page, or 404 when the listing is not the host's.</returns>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var property = await _properties.GetByIdAsync(id);

        if (property is null || property.HostId != userId)
            return NotFound();

        return View(ToEditModel(property));
    }

    /// <summary>
    /// Saves changes to one of the host's listings, optionally appending new photos.
    /// </summary>
    /// <param name="model">The submitted editor form.</param>
    /// <returns>A redirect to the properties page on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditPropertyViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;
        var property = await _properties.GetByIdAsync(model.Id);

        if (property is null || property.HostId != userId)
            return NotFound();

        var usableImages = model.Images.Where(_files.IsValid).ToList();

        if (!ModelState.IsValid)
        {
            model.ExistingImages = property.Images;
            return View(model);
        }

        property.Title = model.Title;
        property.Description = model.Description;
        property.PricePerNight = model.PricePerNight;
        property.CleaningFee = model.CleaningFee;
        property.Address = model.Address;
        property.City = model.City;
        property.Province = model.Province;
        property.PropertyType = model.PropertyType;
        property.MaxGuests = model.MaxGuests;
        property.Bedrooms = model.Bedrooms;
        property.Beds = model.Beds;
        property.Bathrooms = model.Bathrooms;

        if (model.IsActive)
            property.Activate();
        else
            property.Deactivate();

        var hasCover = property.Images.Any(i => i.IsCover);
        foreach (var image in usableImages)
        {
            property.Images.Add(new PropertyImage
            {
                ImageUrl = await _files.SaveAsync(image, "properties"),
                IsCover = !hasCover
            });
            hasCover = true;
        }

        await _properties.UpdateAsync(property);

        await _activityLog.LogAsync(
            userId,
            "PropertyUpdated",
            ActivityType.PropertyUpdated,
            $"Listing '{property.Title}' (#{property.Id}) was updated.");

        TempData["Success"] = "Listing updated.";
        return RedirectToAction(nameof(Properties));
    }

    /// <summary>
    /// Publishes or withdraws one of the host's listings.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <returns>A redirect to the properties page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        if (!await _properties.ToggleActiveAsync(id, userId))
            return NotFound();

        await _activityLog.LogAsync(
            userId,
            "PropertyActivationToggled",
            ActivityType.PropertyUpdated,
            $"Property #{id} activation toggled.");

        return RedirectToAction(nameof(Properties));
    }

    /// <summary>
    /// Lists the booking requests placed against the host's listings.
    /// </summary>
    /// <returns>The booking requests page.</returns>
    [HttpGet]
    public async Task<IActionResult> BookingRequests(int page = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var bookings = await _bookings.GetByHostAsync(userId);
        return View(PagedList<Booking>.Create(bookings, page));
    }

    /// <summary>
    /// Approves a pending booking request.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>A redirect to the booking requests page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveBooking(int id) => await DecideBookingAsync(id, approve: true);

    /// <summary>
    /// Rejects a pending booking request.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>A redirect to the booking requests page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectBooking(int id) => await DecideBookingAsync(id, approve: false);

    /// <summary>
    /// Renders the check-in process builder for one of the host's listings.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <returns>The builder page, or 404 when the listing is not the host's.</returns>
    [HttpGet]
    public async Task<IActionResult> CheckInProcess(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var property = await _properties.GetByIdAsync(id);

        if (property is null || property.HostId != userId)
            return NotFound();

        var existing = await _checkIns.GetProcessByPropertyAsync(id);
        var steps = existing is null
            ? Array.Empty<string>()
            : JsonSerializer.Deserialize<string[]>(existing.StepsJson) ?? Array.Empty<string>();

        return View(new CheckInProcessViewModel
        {
            PropertyId = id,
            PropertyTitle = property.Title,
            Title = existing?.Title ?? "Arrival instructions",
            StepsText = string.Join(Environment.NewLine, steps)
        });
    }

    /// <summary>
    /// Saves the check-in process, serialising the steps into <c>StepsJson</c>.
    /// </summary>
    /// <param name="model">The submitted builder form.</param>
    /// <returns>A redirect to the properties page on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInProcess(CheckInProcessViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;
        var property = await _properties.GetByIdAsync(model.PropertyId);

        if (property is null || property.HostId != userId)
            return NotFound();

        var steps = (model.StepsText ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        if (steps.Length == 0)
            ModelState.AddModelError(nameof(model.StepsText), "Define at least one check-in step.");

        if (!ModelState.IsValid)
        {
            model.PropertyTitle = property.Title;
            return View(model);
        }

        await _checkIns.SaveProcessAsync(new CheckInProcess
        {
            PropertyId = model.PropertyId,
            Title = model.Title,
            StepsJson = JsonSerializer.Serialize(steps),
            CreatedAt = DateTime.UtcNow
        });

        await _activityLog.LogAsync(
            userId,
            "CheckInProcessUpdated",
            ActivityType.PropertyUpdated,
            $"Check-in Process saved for '{property.Title}'.");

        TempData["Success"] = "Check-in Process saved.";
        return RedirectToAction(nameof(Properties));
    }

    /// <summary>
    /// Lists the guest documents awaiting the host's verification.
    /// </summary>
    /// <returns>The verification page.</returns>
    [HttpGet]
    public async Task<IActionResult> Verifications(int page = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var pending = await _checkIns.GetPendingVerificationAsync(userId);
        return View(PagedList<GuestCheckIn>.Create(pending, page));
    }

    /// <summary>
    /// Records the host's decision on a guest document. Accepting the document
    /// advances the booking to CheckedIn.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="bookingId">The booking the document belongs to.</param>
    /// <param name="approved">Whether the host accepted the document.</param>
    /// <returns>A redirect to the verification page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDocument(int documentId, int bookingId, bool approved)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(bookingId);

        if (booking?.Property is null || booking.Property.HostId != userId)
            return NotFound();

        if (!await _checkIns.VerifyDocumentAsync(documentId, approved))
            return NotFound();

        if (approved && booking.CheckIn())
        {
            await _bookings.UpdateAsync(booking);

            await _activityLog.LogAsync(
                userId,
                "BookingCheckedIn",
                ActivityType.BookingStatusUpdate,
                $"Booking #{booking.Id} moved to CheckedIn after document verification.");
        }

        await _notifications.CreateAsync(
            booking.GuestId,
            approved ? "Check-in verified" : "Check-in document rejected",
            approved
                ? "Your host has verified your document. You are checked in."
                : "Your host could not verify your document. Please upload a clearer copy.",
            NotificationType.System,
            "/Bookings");

        TempData["Success"] = approved ? "Document verified." : "Document rejected.";
        return RedirectToAction(nameof(Verifications));
    }

    /// <summary>
    /// Renders the earnings report.
    /// </summary>
    /// <returns>The earnings page.</returns>
    [HttpGet]
    public async Task<IActionResult> Earnings(int page = 1)
    {
        var userId = _userManager.GetUserId(User)!;
        var all = (await _bookings.GetByHostAsync(userId)).ToList();

        // The stat cards summarise every booking, so the full set travels with the
        // page. Paging the model alone would quietly reduce a host's reported
        // earnings to whatever happened to be on screen.
        return View(new HostEarningsViewModel
        {
            All = all,
            Page = PagedList<Booking>.Create(all, page)
        });
    }

    /// <summary>
    /// Renders the reviews left across the host's listings.
    /// </summary>
    /// <returns>The reviews page.</returns>
    [HttpGet]
    public async Task<IActionResult> Reviews()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(await _reviews.GetByHostAsync(userId));
    }

    /// <summary>
    /// Applies an approve or reject decision to a pending booking, logs it, and
    /// notifies the guest.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <param name="approve">Whether the host approved the request.</param>
    /// <returns>A redirect to the booking requests page.</returns>
    private async Task<IActionResult> DecideBookingAsync(int id, bool approve)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(id);

        if (booking?.Property is null || booking.Property.HostId != userId)
            return NotFound();

        var changed = approve ? booking.Approve() : booking.Reject();

        if (!changed)
        {
            TempData["Error"] = "That booking is no longer pending.";
            return RedirectToAction(nameof(BookingRequests));
        }

        await _bookings.UpdateAsync(booking);

        await _activityLog.LogAsync(
            userId,
            approve ? "BookingApproved" : "BookingRejected",
            ActivityType.BookingStatusUpdate,
            $"Booking #{booking.Id} for '{booking.Property.Title}'.");

        await _notifications.CreateAsync(
            booking.GuestId,
            approve ? "Booking Approved" : "Booking Rejected",
            approve
                ? $"Your booking at '{booking.Property.Title}' has been approved. Pay for the stay, then complete check-in."
                : $"Your booking request for '{booking.Property.Title}' was not accepted.",
            approve ? NotificationType.BookingApproved : NotificationType.BookingRejected,
            approve ? "/Bookings" : "/Bookings");

        TempData["Success"] = approve ? "Booking approved." : "Booking rejected.";
        return RedirectToAction(nameof(BookingRequests));
    }

    private static EditPropertyViewModel ToEditModel(HostProperty property) => new()
    {
        Id = property.Id,
        Title = property.Title,
        Description = property.Description,
        PricePerNight = property.PricePerNight,
        CleaningFee = property.CleaningFee,
        Address = property.Address,
        City = property.City,
        Province = property.Province,
        PropertyType = property.PropertyType,
        MaxGuests = property.MaxGuests,
        Bedrooms = property.Bedrooms,
        Beds = property.Beds,
        Bathrooms = property.Bathrooms,
        IsActive = property.IsActive,
        ExistingImages = property.Images
    };
}
