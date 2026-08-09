namespace StayHub.Core.Enums;

/// <summary>
/// Represents progress through the multi-step guest check-in workflow.
/// </summary>
public enum CheckInStatus
{
    /// <summary>The guest has not begun the check-in steps.</summary>
    NotStarted,

    /// <summary>The guest is part-way through the steps.</summary>
    InProgress,

    /// <summary>All steps complete; documents await host verification.</summary>
    PendingVerification,

    /// <summary>The host verified the uploaded documents.</summary>
    Verified,

    /// <summary>The host rejected the uploaded documents.</summary>
    Rejected
}
