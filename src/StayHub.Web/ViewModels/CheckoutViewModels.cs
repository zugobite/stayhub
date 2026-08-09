using System.ComponentModel.DataAnnotations;
using StayHub.Core.Models;
using StayHub.Web.Services;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the sandbox checkout page.
/// </summary>
public class CheckoutViewModel
{
    /// <summary>Gets or sets the booking being paid for.</summary>
    public Booking Booking { get; set; } = null!;

    /// <summary>Gets or sets the gateway's display name.</summary>
    public string GatewayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the cards the sandbox accepts, listed for the guest.</summary>
    public IReadOnlyList<SandboxCard> TestCards { get; set; } = Array.Empty<SandboxCard>();

    /// <summary>Gets or sets the card details being submitted.</summary>
    public CardDetailsViewModel Card { get; set; } = new();

    /// <summary>Gets the number of nights booked.</summary>
    public int Nights => (Booking.CheckOutDate.Date - Booking.CheckInDate.Date).Days;

    /// <summary>Gets the accommodation subtotal, before fees.</summary>
    public decimal Subtotal =>
        Booking.Property?.CalculateSubtotal(Math.Max(Nights, 1), Math.Max(Booking.NumberOfGuests, 1)) ?? 0m;

    /// <summary>Gets the service fee charged on this stay.</summary>
    public decimal ServiceFee =>
        Booking.Property?.CalculateServiceFee(Math.Max(Nights, 1), Math.Max(Booking.NumberOfGuests, 1)) ?? 0m;
}

/// <summary>
/// The card details posted to the sandbox gateway.
/// </summary>
/// <remarks>
/// Only the last four digits are ever persisted. The full number lives no longer
/// than the request that carries it.
/// </remarks>
public class CardDetailsViewModel
{
    /// <summary>Gets or sets the booking being paid for.</summary>
    public int BookingId { get; set; }

    /// <summary>Gets or sets the name printed on the card.</summary>
    [Required(ErrorMessage = "Enter the cardholder name.")]
    [StringLength(100)]
    [Display(Name = "Name on card")]
    public string CardHolder { get; set; } = string.Empty;

    /// <summary>Gets or sets the card number.</summary>
    [Required(ErrorMessage = "Enter a card number.")]
    [StringLength(25)]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the expiry in MM/YY form.</summary>
    [Required(ErrorMessage = "Enter the expiry date.")]
    [RegularExpression(@"^\d{2}\s*/\s*\d{2}$", ErrorMessage = "Use MM/YY.")]
    [Display(Name = "Expiry")]
    public string Expiry { get; set; } = string.Empty;

    /// <summary>Gets or sets the card security code.</summary>
    [Required(ErrorMessage = "Enter the security code.")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Three or four digits.")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}

/// <summary>
/// Backs the receipt shown after a successful charge.
/// </summary>
public class PaymentReceiptViewModel
{
    /// <summary>Gets or sets the booking that was paid for.</summary>
    public Booking Booking { get; set; } = null!;

    /// <summary>Gets or sets the recorded payment.</summary>
    public Payment Payment { get; set; } = null!;

    /// <summary>Gets or sets the gateway's display name.</summary>
    public string GatewayName { get; set; } = string.Empty;
}
