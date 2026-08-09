using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;
using StayHub.Web.Services;
using StayHub.Web.ViewModels;

namespace StayHub.Web.Controllers;

/// <summary>
/// The database-backed inbox with live SignalR delivery to open conversations.
/// </summary>
[Authorize]
public class MessagesController : Controller
{
    private readonly IMessageRepository _messages;
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMessagingPolicyService _policy;
    private readonly IRealtimeDispatchService _realtime;

    /// <summary>
    /// Initialises a new instance of the <see cref="MessagesController"/> class.
    /// </summary>
    public MessagesController(
        IMessageRepository messages,
        INotificationRepository notifications,
        IUserRepository users,
        UserManager<ApplicationUser> userManager,
        IMessagingPolicyService policy,
        IRealtimeDispatchService realtime)
    {
        _messages = messages;
        _notifications = notifications;
        _users = users;
        _userManager = userManager;
        _policy = policy;
        _realtime = realtime;
    }

    /// <summary>
    /// Renders the inbox and sent folders.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        return View(new InboxViewModel
        {
            Threads = await _messages.GetThreadsAsync(userId),
            Sent = await _messages.GetSentAsync(userId),
            UnreadCount = await _messages.GetUnreadCountAsync(userId)
        });
    }

    /// <summary>
    /// Renders one conversation and marks its incoming messages as read.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Conversation(string id)
    {
        var userId = _userManager.GetUserId(User)!;
        var other = await _users.GetByIdAsync(id);

        if (other is null)
            return NotFound();

        var thread = (await _messages.GetConversationAsync(userId, id)).ToList();

        // Always allow opening a thread that already has messages (admin → host, etc.).
        // Policy only blocks starting a brand-new conversation.
        if (thread.Count == 0 && !await _policy.CanMessageAsync(userId, id))
        {
            TempData["Error"] = "You are not allowed to message this user.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var message in thread.Where(m => m.ReceiverId == userId && !m.IsRead))
            await _messages.MarkAsReadAsync(message.Id, userId);

        return View(new ConversationViewModel
        {
            OtherUserId = id,
            OtherUserName = other.FullName,
            Messages = thread
        });
    }

    /// <summary>
    /// Sends a message, raises a notification, and pushes live updates over SignalR.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string receiverId, string content, bool ajax = false)
    {
        var userId = _userManager.GetUserId(User)!;

        if (string.IsNullOrWhiteSpace(content))
        {
            if (ajax || WantsJson())
                return BadRequest(new { error = "The message cannot be empty." });

            TempData["Error"] = "The message cannot be empty.";
            return RedirectToAction(nameof(Conversation), new { id = receiverId });
        }

        if (content.Trim().Length > 2000)
        {
            if (ajax || WantsJson())
                return BadRequest(new { error = "Messages are limited to 2000 characters." });

            TempData["Error"] = "Messages are limited to 2000 characters.";
            return RedirectToAction(nameof(Conversation), new { id = receiverId });
        }

        if (!await _policy.CanMessageAsync(userId, receiverId))
        {
            if (ajax || WantsJson())
                return Forbid();

            return Forbid();
        }

        var receiver = await _users.GetByIdAsync(receiverId);
        if (receiver is null)
            return NotFound();

        var sender = await _users.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("The signed-in user could not be loaded.");

        var message = new Message
        {
            SenderId = userId,
            ReceiverId = receiverId,
            Content = content.Trim(),
            Timestamp = DateTime.UtcNow
        };

        await _messages.CreateAsync(message);

        var notificationId = await _notifications.CreateAsync(
            receiverId,
            "New Message",
            $"You have a new message from {sender.FullName}.",
            NotificationType.NewMessage,
            $"/Messages/Conversation/{userId}");

        await _realtime.NotifyMessageAsync(message, sender);
        await _realtime.NotifyNotificationAsync(new Notification
        {
            Id = notificationId,
            UserId = receiverId,
            Title = "New Message",
            Body = $"You have a new message from {sender.FullName}.",
            Type = NotificationType.NewMessage,
            ActionUrl = $"/Messages/Conversation/{userId}",
            CreatedAt = DateTime.UtcNow
        });

        if (ajax || WantsJson())
        {
            return Json(new
            {
                id = message.Id,
                senderId = message.SenderId,
                receiverId = message.ReceiverId,
                content = message.Content,
                timestamp = message.Timestamp.ToString("o"),
                displayTime = message.Timestamp.ToString("d MMM HH:mm")
            });
        }

        return RedirectToAction(nameof(Conversation), new { id = receiverId });
    }

    private bool WantsJson()
    {
        var accept = Request.Headers.Accept.ToString();
        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
