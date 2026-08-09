namespace StayHub.Core.Models;

/// <summary>
/// Join entity resolving the many-to-many relationship between
/// <see cref="HostProperty"/> and <see cref="Amenity"/>.
/// </summary>
public class PropertyAmenity
{
    /// <summary>Gets or sets the listing's identifier.</summary>
    public int PropertyId { get; set; }

    /// <summary>Gets or sets the listing.</summary>
    public HostProperty? Property { get; set; }

    /// <summary>Gets or sets the amenity's identifier.</summary>
    public int AmenityId { get; set; }

    /// <summary>Gets or sets the amenity.</summary>
    public Amenity? Amenity { get; set; }
}
