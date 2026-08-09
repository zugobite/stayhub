namespace StayHub.Core.Enums;

/// <summary>
/// Categorises a notification shown in the notification centre.
/// </summary>
public enum NotificationType
{
    /// <summary>A host approved the guest's booking request.</summary>
    BookingApproved,

    /// <summary>A host rejected the guest's booking request.</summary>
    BookingRejected,

    /// <summary>A new inbox message arrived.</summary>
    NewMessage,

    /// <summary>An administrator approved the user's host application.</summary>
    HostApplicationApproved,

    /// <summary>An existing host submitted an application for a further property.</summary>
    AdditionalPropertyApplication,

    /// <summary>A general platform announcement.</summary>
    System
}
