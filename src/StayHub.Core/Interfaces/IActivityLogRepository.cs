using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for the system audit trail.
/// </summary>
public interface IActivityLogRepository
{
    /// <summary>
    /// Writes an audit entry.
    /// </summary>
    /// <param name="userId">The acting user's identifier.</param>
    /// <param name="action">The action name, for example <c>HostApplicationSubmitted</c>.</param>
    /// <param name="activityType">The action category.</param>
    /// <param name="details">Optional detail about the affected entity.</param>
    /// <returns>The generated identifier, or <c>0</c> when the user no longer exists.</returns>
    Task<int> LogAsync(string userId, string action, ActivityType activityType, string? details = null);

    /// <summary>
    /// Retrieves audit entries for the administrator's log viewer.
    /// </summary>
    /// <param name="take">The maximum number of entries to return.</param>
    /// <param name="activityType">Optional category filter.</param>
    /// <returns>The matching entries, newest first.</returns>
    Task<IEnumerable<ActivityLog>> GetRecentAsync(int take = 100, ActivityType? activityType = null);
}
