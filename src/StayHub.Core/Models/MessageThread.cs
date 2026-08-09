namespace StayHub.Core.Models;

/// <summary>
/// A conversation preview for the inbox: one row per correspondent.
/// </summary>
public class MessageThread
{
    /// <summary>Gets or sets the other participant's user identifier.</summary>
    public string OtherUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the other participant's display name.</summary>
    public string OtherUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets a short preview of the latest message body.</summary>
    public string Preview { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp of the latest message.</summary>
    public DateTime LastTimestamp { get; set; }

    /// <summary>Gets or sets how many unread messages are waiting from this correspondent.</summary>
    public int UnreadCount { get; set; }
}
