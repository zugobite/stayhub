using StayHub.Core.Enums;

namespace StayHub.Web.Helpers;

/// <summary>
/// Friendly labels and icons for notification types shown in the centre UI.
/// </summary>
public static class NotificationDisplay
{
    /// <summary>Returns a short human-readable label for <paramref name="type"/>.</summary>
    public static string Label(NotificationType type) => type switch
    {
        NotificationType.BookingApproved => "Booking approved",
        NotificationType.BookingRejected => "Booking declined",
        NotificationType.NewMessage => "Message",
        NotificationType.HostApplicationApproved => "Host approved",
        NotificationType.AdditionalPropertyApplication => "Property application",
        NotificationType.System => "Update",
        _ => "Notification"
    };

    /// <summary>Returns the <c>_Icon</c> key for <paramref name="type"/>.</summary>
    public static string Icon(NotificationType type) => type switch
    {
        NotificationType.BookingApproved => "check",
        NotificationType.BookingRejected => "x",
        NotificationType.NewMessage => "mail",
        NotificationType.HostApplicationApproved => "home",
        NotificationType.AdditionalPropertyApplication => "document",
        NotificationType.System => "bell",
        _ => "bell"
    };

    /// <summary>Returns a primary CTA label for a notification with an action URL.</summary>
    public static string ActionLabel(NotificationType type) => type switch
    {
        NotificationType.NewMessage => "Open conversation",
        NotificationType.BookingApproved => "View booking",
        NotificationType.BookingRejected => "View booking",
        NotificationType.HostApplicationApproved => "View listing",
        NotificationType.AdditionalPropertyApplication => "View application",
        _ => "Open"
    };

    /// <summary>Formats a notification timestamp for the list and detail panes.</summary>
    public static string FormatWhen(DateTime createdAtUtc) => TimeDisplay.FormatWhen(createdAtUtc);
}
