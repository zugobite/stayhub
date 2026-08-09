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
/// The guest side of the booking engine: raising requests and reviewing trips.
/// Hosts and administrators are kept out by the role filter.
/// </summary>
[Authorize(Roles = Roles.Guest)]
public class BookingsController : Controller
{
    private readonly IBookingRepository _bookings;
    private readonly IPropertyRepository _properties;
    private readonly INotificationRepository _notifications;
    private readonly IActivityLogRepository _activityLog;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="BookingsController"/> class.
    /// </summary>
    /// <param name="bookings">The booking repository.</param>
    /// <param name="properties">The listing repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public BookingsController(
        IBookingRepository bookings,
        IPropertyRepository properties,
        INotificationRepository notifications,
        IActivityLogRepository activityLog,
        UserManager<ApplicationUser> userManager)
    {
        _bookings = bookings;
        _properties = properties;
        _notifications = notifications;
        _activityLog = activityLog;
        _userManager = userManager;
    }

    /// <summary>
    /// Lists the signed-in guest's trips.
    /// </summary>
    /// <returns>The trips page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        return View(new MyTripsViewModel { Bookings = await _bookings.GetByGuestAsync(userId) });
    }

    /// <summary>
    /// Raises a booking request, pricing it as
    /// <c>(PricePerNight × Nights) + CleaningFee + ServiceFee</c>.
    /// </summary>
    /// <param name="model">The submitted booking widget.</param>
    /// <returns>A redirect to the trips page on success; the detail page otherwise.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingRequestViewModel model)
    {
        var property = await _properties.GetByIdAsync(model.PropertyId);

        if (property is null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the dates you selected.";
            return RedirectToAction(nameof(PropertiesController.View), "Properties", new { id = model.PropertyId });
        }

        if (model.NumberOfGuests > property.MaxGuests)
        {
            TempData["Error"] = $"This property sleeps at most {property.MaxGuests} guests.";
            return RedirectToAction(nameof(PropertiesController.View), "Properties", new { id = model.PropertyId });
        }

        if (!await _bookings.IsAvailableAsync(model.PropertyId, model.CheckInDate, model.CheckOutDate))
        {
            TempData["Error"] = "Those dates are no longer available.";
            return RedirectToAction(nameof(PropertiesController.View), "Properties", new { id = model.PropertyId });
        }

        var userId = _userManager.GetUserId(User)!;
        var nights = (model.CheckOutDate.Date - model.CheckInDate.Date).Days;

        var booking = new Booking
        {
            PropertyId = property.Id,
            GuestId = userId,
            CheckInDate = model.CheckInDate.Date,
            CheckOutDate = model.CheckOutDate.Date,
            NumberOfGuests = model.NumberOfGuests,
            TotalPrice = property.CalculateTotalPrice(nights),
            CreatedAt = DateTime.UtcNow
        };

        var bookingId = await _bookings.CreateAsync(booking);

        await _activityLog.LogAsync(
            userId,
            "BookingRequested",
            ActivityType.BookingStatusUpdate,
            $"Booking #{bookingId} requested for '{property.Title}'.");

        await _notifications.CreateAsync(
            property.HostId,
            "New booking request",
            $"A guest requested {nights} night(s) at '{property.Title}'.",
            NotificationType.System,
            "/Host/BookingRequests");

        TempData["Success"] = "Your booking request has been sent to the host.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Cancels one of the guest's own bookings.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>A redirect to the trips page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var booking = await _bookings.GetByIdAsync(id);

        if (booking is null || booking.GuestId != userId)
            return NotFound();

        if (!booking.Cancel())
        {
            TempData["Error"] = "This booking can no longer be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        await _bookings.UpdateAsync(booking);
        await _activityLog.LogAsync(
            userId,
            "BookingCancelled",
            ActivityType.BookingStatusUpdate,
            $"Booking #{booking.Id} cancelled by the guest.");

        TempData["Success"] = "Your booking has been cancelled.";
        return RedirectToAction(nameof(Index));
    }
}
