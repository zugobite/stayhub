namespace StayHub.Core.Models;

/// <summary>
/// A feature a property can offer, such as WiFi or Air Conditioning.
/// </summary>
public class Amenity
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the icon name rendered beside the name, as understood by the _Icon view partial (for example "wifi").</summary>
    public string IconClass { get; set; } = string.Empty;

    /// <summary>Gets or sets the listings offering this amenity.</summary>
    public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
}
