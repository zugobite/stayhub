using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// An audit-trail entry. One is written for every login, role change, and
/// booking status update so administrators can reconstruct what happened.
/// </summary>
public class ActivityLog
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the acting user's identifier.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the acting user.</summary>
    public ApplicationUser? User { get; set; }

    /// <summary>Gets or sets the action name, for example <c>HostApplicationSubmitted</c>.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets the action category.</summary>
    public ActivityType ActivityType { get; set; }

    /// <summary>Gets or sets optional free-text detail about the affected entity.</summary>
    public string? Details { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the action.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
