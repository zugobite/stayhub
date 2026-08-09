namespace StayHub.Core.Models;

/// <summary>
/// A photograph attached to a listing. At least one is mandatory before a
/// host application can be submitted.
/// </summary>
public class PropertyImage
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the owning listing's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the owning listing.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the relative URL of the stored image.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this is the gallery cover image.</summary>
    public bool IsCover { get; set; }
}
