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
/// Lets guests leave a review after a completed stay.
/// </summary>
[Authorize(Roles = Roles.Guest)]
public class ReviewsController : Controller
{
    private readonly IBookingRepository _bookings;
    private readonly IReviewRepository _reviews;
    private readonly INotificationRepository _notifications;
    private readonly IActivityLogRepository _activityLog;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="ReviewsController"/> class.
    /// </summary>
    public ReviewsController(
        IBookingRepository bookings,
        IReviewRepository reviews,
        INotificationRepository notifications,
        IActivityLogRepository activityLog,
        UserManager<ApplicationUser> userManager)
    {
        _bookings = bookings;
        _reviews = reviews;
        _notifications = notifications;
        _activityLog = activityLog;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the leave-a-review form for a completed booking.
    /// </summary>
    /// <param name="bookingId">The completed booking identifier.</param>
    /// <returns>The review form, or a redirect when the booking is not eligible.</returns>
    [HttpGet]
    public async Task<IActionResult> Create(int bookingId)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(bookingId);

        if (booking is null || booking.GuestId != userId || booking.Property is null)
            return NotFound();

        if (booking.Status != BookingStatus.Completed)
        {
            TempData["Error"] = "You can leave a review after the stay is marked complete.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        if (await _reviews.ExistsAsync(booking.PropertyId, userId))
        {
            TempData["Error"] = "You have already reviewed this listing.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        return View(new LeaveReviewViewModel
        {
            BookingId = booking.Id,
            PropertyId = booking.PropertyId,
            PropertyTitle = booking.Property.Title
        });
    }

    /// <summary>
    /// Saves a guest review for a completed stay.
    /// </summary>
    /// <param name="model">The submitted review form.</param>
    /// <returns>A redirect to trips on success; the form otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveReviewViewModel model)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(model.BookingId);

        if (booking is null || booking.GuestId != userId || booking.Property is null)
            return NotFound();

        if (booking.Status != BookingStatus.Completed)
        {
            TempData["Error"] = "You can leave a review after the stay is marked complete.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        if (await _reviews.ExistsAsync(booking.PropertyId, userId))
        {
            TempData["Error"] = "You have already reviewed this listing.";
            return RedirectToAction(nameof(BookingsController.Index), "Bookings");
        }

        if (!ModelState.IsValid)
        {
            model.PropertyTitle = booking.Property.Title;
            model.PropertyId = booking.PropertyId;
            return View(model);
        }

        await _reviews.CreateAsync(new Review
        {
            PropertyId = booking.PropertyId,
            ReviewerId = userId,
            Rating = model.Rating,
            Comment = model.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _activityLog.LogAsync(
            userId,
            "ReviewSubmitted",
            ActivityType.PropertyUpdated,
            $"Review left for '{booking.Property.Title}' ({model.Rating}/5).");

        await _notifications.CreateAsync(
            booking.Property.HostId,
            "New review",
            $"A guest rated '{booking.Property.Title}' {model.Rating}/5.",
            NotificationType.System,
            "/Host/Reviews");

        TempData["Success"] = "Thanks for your review.";
        return RedirectToAction(nameof(BookingsController.Index), "Bookings");
    }
}
