namespace StayHub.Core.Enums;

/// <summary>
/// Represents the settlement state of a booking payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Payment record created but not yet settled.</summary>
    Pending,

    /// <summary>Payment settled successfully.</summary>
    Paid,

    /// <summary>Payment attempt failed.</summary>
    Failed,

    /// <summary>Payment was returned to the guest.</summary>
    Refunded
}
