namespace StayHub.Core.Models;

/// <summary>
/// A database-backed inbox message between two users. Delivery is not real-time;
/// the brief explicitly excludes SignalR.
/// </summary>
public class Message
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the sender's identifier.</summary>
    public string SenderId { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender.</summary>
    public ApplicationUser? Sender { get; set; }

    /// <summary>Gets or sets the recipient's identifier.</summary>
    public string ReceiverId { get; set; } = string.Empty;

    /// <summary>Gets or sets the recipient.</summary>
    public ApplicationUser? Receiver { get; set; }

    /// <summary>Gets or sets the message body.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC send timestamp.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the recipient has opened the message.
    /// Persisted so the inbox read/unread state survives a restart.
    /// </summary>
    public bool IsRead { get; set; }
}
