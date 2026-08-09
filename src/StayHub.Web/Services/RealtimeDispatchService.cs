using Microsoft.AspNetCore.SignalR;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Hubs;

namespace StayHub.Web.Services;

/// <summary>
/// SignalR-backed implementation of <see cref="IRealtimeDispatchService"/>.
/// </summary>
public class RealtimeDispatchService : IRealtimeDispatchService
{
    private readonly IHubContext<StayHubHub> _hub;
    private readonly INotificationRepository _notifications;
    private readonly IMessageRepository _messages;

    /// <summary>
    /// Initialises a new instance of the <see cref="RealtimeDispatchService"/> class.
    /// </summary>
    public RealtimeDispatchService(
        IHubContext<StayHubHub> hub,
        INotificationRepository notifications,
        IMessageRepository messages)
    {
        _hub = hub;
        _notifications = notifications;
        _messages = messages;
    }

    /// <inheritdoc />
    public async Task NotifyMessageAsync(Message message, ApplicationUser sender)
    {
        var payload = new
        {
            id = message.Id,
            senderId = message.SenderId,
            receiverId = message.ReceiverId,
            senderName = sender.FullName,
            content = message.Content,
            timestamp = message.Timestamp.ToString("o"),
            displayTime = message.Timestamp.ToString("d MMM HH:mm")
        };

        await _hub.Clients.User(message.ReceiverId).SendAsync("messageReceived", payload);
        await _hub.Clients.User(message.SenderId).SendAsync("messageReceived", payload);

        var unreadMessages = await _messages.GetUnreadCountAsync(message.ReceiverId);
        await _hub.Clients.User(message.ReceiverId).SendAsync("messageBadgeUpdated", new
        {
            unreadCount = unreadMessages
        });
    }

    /// <inheritdoc />
    public async Task NotifyNotificationAsync(Notification notification)
    {
        var unreadCount = await _notifications.GetUnreadCountAsync(notification.UserId);

        await _hub.Clients.User(notification.UserId).SendAsync("notificationReceived", new
        {
            id = notification.Id,
            title = notification.Title,
            body = notification.Body,
            actionUrl = notification.ActionUrl,
            type = notification.Type.ToString(),
            unreadCount
        });
    }
}
