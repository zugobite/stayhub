using System.ComponentModel.DataAnnotations;
using StayHub.Core.Models;

namespace StayHub.Web.ViewModels;

/// <summary>
/// Backs the notification centre: the ALL / UNREAD / READ / TODAY filters on the
/// left and the selected notification's detail on the right.
/// </summary>
public class NotificationCentreViewModel
{
    /// <summary>Gets or sets the notifications matching the active filter.</summary>
    public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>Gets or sets the notification shown in the detail pane.</summary>
    public Notification? Selected { get; set; }

    /// <summary>Gets or sets the active filter: <c>all</c>, <c>unread</c>, <c>read</c>, or <c>today</c>.</summary>
    public string Filter { get; set; } = "all";

    /// <summary>Gets or sets the total notification count.</summary>
    public int AllCount { get; set; }

    /// <summary>Gets or sets the unread count.</summary>
    public int UnreadCount { get; set; }

    /// <summary>Gets or sets the read count.</summary>
    public int ReadCount { get; set; }

    /// <summary>Gets or sets the count raised today.</summary>
    public int TodayCount { get; set; }
}

/// <summary>
/// Backs the message inbox.
/// </summary>
public class InboxViewModel
{
    /// <summary>Gets or sets one conversation preview per correspondent.</summary>
    public IEnumerable<MessageThread> Threads { get; set; } = new List<MessageThread>();

    /// <summary>Gets or sets the sent messages.</summary>
    public IEnumerable<Message> Sent { get; set; } = new List<Message>();

    /// <summary>Gets or sets the unread count for the header badge.</summary>
    public int UnreadCount { get; set; }
}

/// <summary>
/// Backs a single conversation thread and its reply box.
/// </summary>
public class ConversationViewModel
{
    /// <summary>Gets or sets the correspondent's user identifier.</summary>
    public string OtherUserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the correspondent's display name.</summary>
    public string OtherUserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the exchange, oldest first.</summary>
    public IEnumerable<Message> Messages { get; set; } = new List<Message>();

    /// <summary>Gets or sets the reply being composed.</summary>
    [Required, StringLength(2000)]
    public string Reply { get; set; } = string.Empty;
}
