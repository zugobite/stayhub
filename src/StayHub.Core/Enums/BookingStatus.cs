namespace StayHub.Core.Enums;

/// <summary>
/// Represents the lifecycle state of a guest booking.
/// </summary>
public enum BookingStatus
{
    /// <summary>Booking has been requested and awaits host review.</summary>
    Pending,

    /// <summary>Host has approved the booking request.</summary>
    Approved,

    /// <summary>Host has rejected the booking request.</summary>
    Rejected,

    /// <summary>Payment for the booking has been captured.</summary>
    Paid,

    /// <summary>Guest has completed check-in and the host verified their documents.</summary>
    CheckedIn,

    /// <summary>The stay has ended.</summary>
    Completed,

    /// <summary>The booking was cancelled by the guest.</summary>
    Cancelled
}
