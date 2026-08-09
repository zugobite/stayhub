using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// The single payment raised against a booking. No gateway is integrated;
/// the status is advanced manually, which the brief permits.
/// </summary>
public class Payment
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the settled booking's identifier.</summary>
    public int BookingId { get; set; }

    /// <summary>Gets or sets the settled booking.</summary>
    public Booking? Booking { get; set; }

    /// <summary>Gets or sets the amount in ZAR.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the settlement state.</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
