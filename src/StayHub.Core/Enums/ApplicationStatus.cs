namespace StayHub.Core.Enums;

/// <summary>
/// Represents the review state of a host application.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>Submitted and awaiting administrator review.</summary>
    Pending,

    /// <summary>Approved by an administrator; roles are swapped on approval.</summary>
    Approved,

    /// <summary>Rejected by an administrator.</summary>
    Rejected
}
