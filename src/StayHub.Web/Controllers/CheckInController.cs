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
/// The guest's multi-step check-in wizard. The steps come from the property's
/// <see cref="CheckInProcess"/>; the final step uploads an identity document
/// for the host to verify.
/// </summary>
[Authorize(Roles = Roles.Guest)]
public class CheckInController : Controller
{
    private readonly ICheckInRepository _checkIns;
    private readonly IBookingRepository _bookings;
    private readonly INotificationRepository _notifications;
    private readonly IActivityLogRepository _activityLog;
    private readonly IFileStorageService _files;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="CheckInController"/> class.
    /// </summary>
    /// <param name="checkIns">The check-in repository.</param>
    /// <param name="bookings">The booking repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="files">The upload storage service.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public CheckInController(
        ICheckInRepository checkIns,
        IBookingRepository bookings,
        INotificationRepository notifications,
        IActivityLogRepository activityLog,
        IFileStorageService files,
        UserManager<ApplicationUser> userManager)
    {
        _checkIns = checkIns;
        _bookings = bookings;
        _notifications = notifications;
        _activityLog = activityLog;
        _files = files;
        _userManager = userManager;
    }

    /// <summary>
    /// Opens the wizard for an approved booking, creating the check-in record on
    /// first visit.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <param name="step">The zero-based step to display.</param>
    /// <returns>The wizard page, or 404 when the booking is not the guest's.</returns>
    [HttpGet]
    public async Task<IActionResult> Start(int id, int step = 0)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(id);

        if (booking is null || booking.GuestId != userId)
            return NotFound();

        if (booking.Status is not (BookingStatus.Approved or BookingStatus.Paid))
        {
            TempData["Error"] = "Check-in opens once your host approves the booking.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        var process = await _checkIns.GetProcessByPropertyAsync(booking.PropertyId);

        if (process is null)
        {
            TempData["Error"] = "Your host has not published a check-in process for this property yet.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        var checkIn = await _checkIns.GetCheckInByBookingAsync(id);

        if (checkIn is null)
        {
            checkIn = new GuestCheckIn
            {
                BookingId = id,
                CheckInProcessId = process.Id,
                Status = CheckInStatus.InProgress,
                CurrentStep = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _checkIns.CreateCheckInAsync(checkIn);
        }

        var steps = JsonSerializer.Deserialize<string[]>(process.StepsJson) ?? Array.Empty<string>();

        return View(new GuestCheckInViewModel
        {
            Booking = booking,
            CheckIn = checkIn,
            Steps = steps,
            CurrentStep = Math.Clamp(step, 0, steps.Length)
        });
    }

    /// <summary>
    /// Records that the guest completed a step and moves the wizard forward.
    /// </summary>
    /// <param name="bookingId">The booking identifier.</param>
    /// <param name="step">The step just completed.</param>
    /// <returns>A redirect to the next step.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Advance(int bookingId, int step)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(bookingId);

        if (booking is null || booking.GuestId != userId)
            return NotFound();

        var checkIn = await _checkIns.GetCheckInByBookingAsync(bookingId);

        if (checkIn is null)
            return RedirectToAction(nameof(Start), new { id = bookingId });

        checkIn.CurrentStep = step + 1;
        checkIn.Status = CheckInStatus.InProgress;
        await _checkIns.UpdateCheckInAsync(checkIn);

        return RedirectToAction(nameof(Start), new { id = bookingId, step = step + 1 });
    }

    /// <summary>
    /// Stores the guest's identity document and hands the check-in to the host
    /// for verification.
    /// </summary>
    /// <param name="bookingId">The booking identifier.</param>
    /// <param name="documentType">The document kind, for example "Passport".</param>
    /// <param name="document">The uploaded file.</param>
    /// <returns>A redirect to the trips page on success; back to the wizard otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int bookingId, string documentType, IFormFile? document)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(bookingId);

        if (booking is null || booking.GuestId != userId)
            return NotFound();

        if (!_files.IsValid(document))
        {
            TempData["Error"] = "Upload a JPG, PNG, or PDF copy of your ID or passport, up to 5 MB.";
            return RedirectToAction(nameof(Start), new { id = bookingId, step = int.MaxValue });
        }

        var checkIn = await _checkIns.GetCheckInByBookingAsync(bookingId);

        if (checkIn is null)
            return RedirectToAction(nameof(Start), new { id = bookingId });

        var url = await _files.SaveAsync(document!, "documents");

        await _checkIns.AddDocumentAsync(new GuestDocument
        {
            GuestCheckInId = checkIn.Id,
            DocumentType = string.IsNullOrWhiteSpace(documentType) ? "ID" : documentType,
            FileName = document!.FileName,
            FileUrl = url,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTime.UtcNow
        });

        checkIn.Status = CheckInStatus.PendingVerification;
        await _checkIns.UpdateCheckInAsync(checkIn);

        await _activityLog.LogAsync(
            userId,
            "GuestDocumentUploaded",
            ActivityType.BookingStatusUpdate,
            $"Document uploaded for booking #{bookingId}.");

        if (booking.Property is not null)
        {
            await _notifications.CreateAsync(
                booking.Property.HostId,
                "Guest document awaiting verification",
                $"A guest uploaded their {documentType} for '{booking.Property.Title}'.",
                NotificationType.System,
                "/Host/Verifications");
        }

        TempData["Success"] = "Your document was uploaded and is awaiting host verification.";
        return RedirectToAction(nameof(BookingsController.Index), "Bookings");
    }
}
