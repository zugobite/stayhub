namespace StayHub.Web.Helpers;

/// <summary>
/// Shared relative / compact timestamp formatting for list UIs.
/// </summary>
public static class TimeDisplay
{
    /// <summary>Formats a timestamp for inbox, notifications, and similar lists.</summary>
    public static string FormatWhen(DateTime createdAtUtc)
    {
        var local = createdAtUtc.Kind == DateTimeKind.Utc
            ? createdAtUtc.ToLocalTime()
            : createdAtUtc;
        var now = DateTime.Now;
        var span = now - local;

        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (local.Date == now.Date) return local.ToString("HH:mm");
        if (local.Date == now.Date.AddDays(-1)) return $"Yesterday {local:HH:mm}";
        if (local.Year == now.Year) return local.ToString("d MMM · HH:mm");
        return local.ToString("d MMM yyyy · HH:mm");
    }
}
