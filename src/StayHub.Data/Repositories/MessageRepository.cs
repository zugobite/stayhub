using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IMessageRepository"/>.
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="MessageRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public MessageRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Message>> GetInboxAsync(string userId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ReceiverId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Message>> GetSentAsync(string userId)
    {
        return await _context.Messages
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MessageThread>> GetThreadsAsync(string userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        return messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(group =>
            {
                var latest = group.First();
                var other = latest.SenderId == userId ? latest.Receiver : latest.Sender;

                return new MessageThread
                {
                    OtherUserId = group.Key,
                    OtherUserName = other?.FullName ?? "StayHub user",
                    Preview = latest.Content,
                    LastTimestamp = latest.Timestamp,
                    UnreadCount = group.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(t => t.LastTimestamp)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Message>> GetConversationAsync(string userId, string otherUserId)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Messages.CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message.Id;
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsReadAsync(int id, string userId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id && m.ReceiverId == userId);

        if (message is null || message.IsRead)
            return false;

        message.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
