namespace StayHub.Core.Models;

/// <summary>
/// A guest's star rating and comment on a property.
/// </summary>
public class Review
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the reviewed property's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the reviewed property.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the reviewer's identifier.</summary>
    public string ReviewerId { get; set; } = string.Empty;

    /// <summary>Gets or sets the reviewer.</summary>
    public ApplicationUser? Reviewer { get; set; }

    /// <summary>Gets or sets the rating, from 1 to 5.</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the written feedback.</summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
