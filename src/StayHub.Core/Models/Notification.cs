using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// An entry in a user's notification centre.
/// </summary>
public class Notification
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the recipient's identifier.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient.</summary>
    public ApplicationUser? User { get; set; }

    /// <summary>Gets or sets the headline, for example "Host Application Approved".</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the body copy shown in the detail pane.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Gets or sets the notification category.</summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Gets or sets the relative URL the "Take Action" button navigates to.
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the recipient has read the notification.
    /// Drives the ALL / UNREAD / READ / TODAY filters.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
