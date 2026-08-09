namespace StayHub.Core.Enums;

/// <summary>
/// Represents the verification state of a guest-supplied identity document.
/// </summary>
public enum DocumentStatus
{
    /// <summary>Uploaded and awaiting host review.</summary>
    Pending,

    /// <summary>Accepted by the host.</summary>
    Verified,

    /// <summary>Rejected by the host.</summary>
    Rejected
}
