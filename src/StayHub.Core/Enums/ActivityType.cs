namespace StayHub.Core.Enums;

/// <summary>
/// Categorises an entry in the system activity log.
/// </summary>
public enum ActivityType
{
    /// <summary>A user signed in.</summary>
    Login,

    /// <summary>A user signed out.</summary>
    Logout,

    /// <summary>A user's roles were changed (for example Guest swapped for Host).</summary>
    RoleChange,

    /// <summary>A booking moved between statuses.</summary>
    BookingStatusUpdate,

    /// <summary>A host or additional-property application was submitted.</summary>
    ApplicationSubmitted,

    /// <summary>An administrator approved or rejected an application.</summary>
    ApplicationReviewed,

    /// <summary>A property listing was created, edited, or toggled.</summary>
    PropertyUpdated
}
