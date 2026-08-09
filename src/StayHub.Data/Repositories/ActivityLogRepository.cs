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
