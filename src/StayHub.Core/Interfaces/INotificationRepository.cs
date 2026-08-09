using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for the notification centre.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Retrieves a user's notifications, newest first.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <returns>The user's notifications.</returns>
    Task<IEnumerable<Notification>> GetByUserAsync(string userId);

    /// <summary>
    /// Retrieves a single notification for the detail pane.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="userId">The recipient, verified before the record is returned.</param>
    /// <returns>The notification, or <c>null</c> when it does not exist or belongs to another user.</returns>
    Task<Notification?> GetByIdAsync(int id, string userId);

    /// <summary>
    /// Counts a user's unread notifications for the bell badge.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <returns>The unread count.</returns>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>
    /// Raises a notification for a user.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <param name="title">The headline.</param>
    /// <param name="body">The body copy.</param>
    /// <param name="type">The notification category.</param>
    /// <param name="actionUrl">Optional relative URL for the "Take Action" button.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(string userId, string title, string body, NotificationType type, string? actionUrl = null);

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="userId">The recipient, verified before the flag is written.</param>
    /// <returns><c>true</c> when the flag was written; otherwise <c>false</c>.</returns>
    Task<bool> MarkAsReadAsync(int id, string userId);

    /// <summary>
    /// Marks every one of a user's notifications as read.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <returns>The number of notifications updated.</returns>
    Task<int> MarkAllAsReadAsync(string userId);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification identifier.</param>
    /// <param name="userId">The recipient, verified before the delete is applied.</param>
    /// <returns><c>true</c> when the notification was deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(int id, string userId);
}
