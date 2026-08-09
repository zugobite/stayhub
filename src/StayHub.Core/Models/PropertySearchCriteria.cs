using StayHub.Core.Enums;

namespace StayHub.Core.Models;

/// <summary>
/// The filters a guest can apply to the property gallery. Every member is
/// optional; an instance with nothing set returns all active listings.
/// </summary>
/// <remarks>
/// Passing the filters as one object keeps <see cref="Interfaces.IPropertyRepository"/>
/// from growing a new parameter each time the gallery gains a control.
/// </remarks>
public class PropertySearchCriteria
{
    /// <summary>Gets or sets free text matched against title, description, city and address.</summary>
    public string? Keyword { get; set; }

    /// <summary>Gets or sets the city filter.</summary>
    public string? City { get; set; }

    /// <summary>Gets or sets the province filter.</summary>
    public Province? Province { get; set; }

    /// <summary>Gets or sets the listing category filter.</summary>
    public PropertyType? PropertyType { get; set; }

    /// <summary>Gets or sets the lowest acceptable nightly rate in ZAR.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Gets or sets the highest acceptable nightly rate in ZAR.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Gets or sets the number of travellers the listing must sleep.</summary>
    public int? Guests { get; set; }

    /// <summary>Gets or sets the minimum bedroom count.</summary>
    public int? Bedrooms { get; set; }

    /// <summary>Gets or sets the minimum bathroom count.</summary>
    public int? Bathrooms { get; set; }

    /// <summary>
    /// Gets or sets amenity identifiers the listing must offer. A listing must
    /// carry every requested amenity, not merely one of them.
    /// </summary>
    public IReadOnlyCollection<int> AmenityIds { get; set; } = Array.Empty<int>();

    /// <summary>Gets or sets the ordering applied to the results.</summary>
    public PropertySortOrder SortOrder { get; set; } = PropertySortOrder.Newest;

    /// <summary>
    /// Gets a value indicating whether any filter is active, used by the gallery
    /// to decide whether to offer a "clear filters" action.
    /// </summary>
    public bool HasAnyFilter =>
        !string.IsNullOrWhiteSpace(Keyword)
        || !string.IsNullOrWhiteSpace(City)
        || Province.HasValue
        || PropertyType.HasValue
        || MinPrice.HasValue
        || MaxPrice.HasValue
        || Guests.HasValue
        || Bedrooms.HasValue
        || Bathrooms.HasValue
        || AmenityIds.Count > 0;
}
