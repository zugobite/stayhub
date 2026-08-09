using StayHub.Core.Models;

namespace StayHub.Web.Services;

/// <summary>
/// Pushes live message and notification events over SignalR after they are
/// persisted to the database.
/// </summary>
public interface IRealtimeDispatchService
{
    /// <summary>
    /// Notifies the recipient (and the sender's other tabs) that a message arrived.
    /// </summary>
    /// <param name="message">The persisted message.</param>
    /// <param name="sender">The sender, used for display name in the payload.</param>
    Task NotifyMessageAsync(Message message, ApplicationUser sender);

    /// <summary>
    /// Notifies a user that a new notification was raised, including the fresh unread count.
    /// </summary>
    /// <param name="notification">The persisted notification.</param>
    Task NotifyNotificationAsync(Notification notification);
}
