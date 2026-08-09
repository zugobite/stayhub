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

    /// <summary>
    /// Gets or sets the reference issued by the simulated gateway, shown on the
    /// receipt so a guest can quote it.
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last four digits of the card used. Only the last four are
    /// ever stored — the simulation deliberately keeps no other card data.
    /// </summary>
    public string CardLast4 { get; set; } = string.Empty;

    /// <summary>Gets or sets the gateway's message when a charge is declined.</summary>
    public string? FailureReason { get; set; }

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
