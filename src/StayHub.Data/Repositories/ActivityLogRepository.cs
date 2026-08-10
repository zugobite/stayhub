using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IActivityLogRepository"/>.
/// </summary>
public class ActivityLogRepository : IActivityLogRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="ActivityLogRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public ActivityLogRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> LogAsync(string userId, string action, ActivityType activityType, string? details = null)
    {
        // Skip quietly when the cookie still holds an id from a previous database
        // (common after a local drop/reseed) so audit writes never break the request.
        if (string.IsNullOrWhiteSpace(userId) ||
            !await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId))
        {
            return 0;
        }

        var entry = new ActivityLog
        {
            UserId = userId,
            Action = action,
            ActivityType = activityType,
            Details = details,
            CreatedAt = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(entry);
        await _context.SaveChangesAsync();
        return entry.Id;
    }

    /// <inheritdoc />
    public async Task<PagedList<ActivityLog>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        ActivityType? activityType = null)
    {
        var query = _context.ActivityLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        if (activityType.HasValue)
            query = query.Where(l => l.ActivityType == activityType.Value);

        var total = await query.CountAsync();
        var page = PagedList<ActivityLog>.Clamp(pageNumber, total, pageSize);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedList<ActivityLog>.From(items, page, pageSize, total);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int take = 100, ActivityType? activityType = null)
    {
        var query = _context.ActivityLogs
            .AsNoTracking()
            .Include(l => l.User)
            .AsQueryable();

        if (activityType.HasValue)
            query = query.Where(l => l.ActivityType == activityType.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToListAsync();
    }
}
