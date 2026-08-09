namespace StayHub.Core.Models;

/// <summary>
/// A listing a user has saved from the gallery's wishlist heart.
/// </summary>
public class WishlistItem
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the saving user's identifier.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the saving user.</summary>
    public ApplicationUser? User { get; set; }

    /// <summary>Gets or sets the saved listing's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the saved listing.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the listing was saved.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
