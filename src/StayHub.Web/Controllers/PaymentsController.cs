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
/// Drives the simulated card checkout for an approved booking.
/// </summary>
/// <remarks>
/// No money moves and no card data leaves the request. The flow exists to model a
/// real hand-off: the guest leaves the booking pages for a gateway-styled
/// checkout, the charge is authorised or declined, and they return to a receipt.
/// </remarks>
[Authorize(Roles = Roles.Guest)]
public class PaymentsController : Controller
{
    private readonly IBookingRepository _bookings;
    private readonly IActivityLogRepository _activityLog;
    private readonly INotificationRepository _notifications;
    private readonly IPaymentGateway _gateway;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initialises a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="bookings">The booking repository.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    /// <param name="notifications">The notification repository.</param>
    /// <param name="gateway">The card gateway.</param>
    /// <param name="userManager">The Identity user manager.</param>
    public PaymentsController(
        IBookingRepository bookings,
        IActivityLogRepository activityLog,
        INotificationRepository notifications,
        IPaymentGateway gateway,
        UserManager<ApplicationUser> userManager)
    {
        _bookings = bookings;
        _activityLog = activityLog;
        _notifications = notifications;
        _gateway = gateway;
        _userManager = userManager;
    }

    /// <summary>
    /// Renders the sandbox checkout for an approved booking, pre-filled with the
    /// gateway's default test card.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>The checkout page.</returns>
    [HttpGet]
    public async Task<IActionResult> Checkout(int id)
    {
        var booking = await LoadOwnBookingAsync(id);

        if (booking is null)
            return NotFound();

        if (booking.Status != BookingStatus.Approved)
        {
            TempData["Error"] = "Only an approved booking can be paid.";
            return RedirectToAction("Index", "Bookings");
        }

        var user = await _userManager.GetUserAsync(User);

        return View(BuildCheckout(booking, new CardDetailsViewModel
        {
            BookingId = booking.Id,
            CardHolder = user?.FullName ?? string.Empty,
            CardNumber = _gateway.DefaultCard.Number,
            Expiry = "12/30",
            Cvv = "123"
        }));
    }

    /// <summary>
    /// Submits the card to the gateway. On approval the payment is recorded and
    /// the booking advances to Paid; on decline the booking is left untouched so
    /// the guest can try again.
    /// </summary>
    /// <param name="card">The submitted card details.</param>
    /// <returns>The receipt on success, or the checkout page with the decline reason.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(CardDetailsViewModel card)
    {
        var booking = await LoadOwnBookingAsync(card.BookingId);

        if (booking is null || booking.Property is null)
            return NotFound();

        if (booking.Status != BookingStatus.Approved)
        {
            TempData["Error"] = "Only an approved booking can be paid.";
            return RedirectToAction("Index", "Bookings");
        }

        if (!ModelState.IsValid)
            return View(nameof(Checkout), BuildCheckout(booking, card));

        var result = await _gateway.AuthoriseAsync(new GatewayRequest(
            booking.TotalPrice,
            card.CardNumber,
            card.CardHolder,
            card.Expiry,
            card.Cvv,
            $"StayHub booking #{booking.Id} — {booking.Property.Title}"));

        var userId = _userManager.GetUserId(User)!;

        if (!result.Approved)
        {
            // A declined charge is still worth recording: it explains to the guest
            // and to an administrator why a booking sat unpaid.
            booking.Payment = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                Status = PaymentStatus.Failed,
                Reference = result.Reference,
                CardLast4 = result.CardLast4,
                FailureReason = result.FailureReason,
                CreatedAt = DateTime.UtcNow
            };

            await _bookings.UpdateAsync(booking);

            await _activityLog.LogAsync(
                userId,
                "PaymentDeclined",
                ActivityType.BookingStatusUpdate,
                $"Booking #{booking.Id} payment declined ({result.Reference}).");

            ModelState.AddModelError(string.Empty, result.FailureReason ?? "The charge was declined.");
            return View(nameof(Checkout), BuildCheckout(booking, card));
        }

        if (!booking.MarkPaid())
        {
            TempData["Error"] = "That booking can no longer be paid.";
            return RedirectToAction("Index", "Bookings");
        }

        booking.Payment = new Payment
        {
            BookingId = booking.Id,
            Amount = booking.TotalPrice,
            Status = PaymentStatus.Paid,
            Reference = result.Reference,
            CardLast4 = result.CardLast4,
            CreatedAt = DateTime.UtcNow
        };

        await _bookings.UpdateAsync(booking);

        await _activityLog.LogAsync(
            userId,
            "BookingPaid",
            ActivityType.BookingStatusUpdate,
            $"Booking #{booking.Id} paid, R {booking.TotalPrice:N2} ({result.Reference}).");

        await _notifications.CreateAsync(
            booking.Property.HostId,
            "Booking paid",
            $"A guest paid R {booking.TotalPrice:N2} for '{booking.Property.Title}'.",
            NotificationType.System,
            "/Host/Earnings");

        return RedirectToAction(nameof(Receipt), new { id = booking.Id });
    }

    /// <summary>
    /// Renders the receipt for a paid booking.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>The receipt page.</returns>
    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var booking = await LoadOwnBookingAsync(id);

        if (booking?.Payment is null)
            return NotFound();

        return View(new PaymentReceiptViewModel
        {
            Booking = booking,
            Payment = booking.Payment,
            GatewayName = _gateway.Name
        });
    }

    /// <summary>
    /// Loads a booking only when it belongs to the signed-in guest, so guessing an
    /// identifier cannot expose or charge someone else's stay.
    /// </summary>
    private async Task<Booking?> LoadOwnBookingAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        var booking = await _bookings.GetByIdAsync(id);

        return booking is null || booking.GuestId != userId ? null : booking;
    }

    private CheckoutViewModel BuildCheckout(Booking booking, CardDetailsViewModel card) => new()
    {
        Booking = booking,
        GatewayName = _gateway.Name,
        TestCards = _gateway.TestCards,
        Card = card
    };
}
