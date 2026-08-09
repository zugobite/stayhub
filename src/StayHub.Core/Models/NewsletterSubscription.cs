namespace StayHub.Core.Models;

/// <summary>
/// An email address subscribed to StayHub's marketing newsletter.
/// </summary>
public class NewsletterSubscription
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the subscriber's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC subscription timestamp.</summary>
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}
