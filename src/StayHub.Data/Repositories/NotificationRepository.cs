using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="INotificationRepository"/>.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="NotificationRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public NotificationRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Notification>> GetByUserAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Notification?> GetByIdAsync(int id, string userId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(
        string userId,
        string title,
        string body,
        NotificationType type,
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification.Id;
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsReadAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is null || notification.IsRead)
            return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<int> MarkAllAsReadAsync(string userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
            notification.IsRead = true;

        await _context.SaveChangesAsync();
        return unread.Count;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }
}
