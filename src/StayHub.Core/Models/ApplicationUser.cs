using Microsoft.AspNetCore.Identity;

namespace StayHub.Core.Models;

/// <summary>
/// The platform user. Extends <see cref="IdentityUser"/> so ASP.NET Core Identity
/// owns authentication, while the profile fields required by the brief live here.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's surname.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative URL of the user's avatar image.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the host's self-written introduction, shown on the listing
    /// detail page so guests can see who they would be staying with.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the town or city the user calls home, shown beside the bio.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the account was registered.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Gets or sets the properties this user hosts.</summary>
    public ICollection<HostProperty> Properties { get; set; } = new List<HostProperty>();

    /// <summary>Gets or sets the bookings this user has made as a guest.</summary>
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>Gets or sets the reviews this user has written.</summary>
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    /// <summary>Gets or sets the properties this user has saved.</summary>
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();

    /// <summary>Gets or sets the host applications this user has submitted.</summary>
    public ICollection<HostApplication> HostApplications { get; set; } = new List<HostApplication>();

    /// <summary>Gets or sets the notifications addressed to this user.</summary>
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
