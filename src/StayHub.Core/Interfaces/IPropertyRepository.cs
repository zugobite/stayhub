using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for property listings.
/// </summary>
public interface IPropertyRepository
{
    /// <summary>
    /// Retrieves the active listings shown in the guest gallery, optionally filtered.
    /// </summary>
    /// <param name="city">Optional city filter.</param>
    /// <param name="propertyType">Optional listing category filter.</param>
    /// <returns>The matching active listings with their images and reviews.</returns>
    Task<IEnumerable<HostProperty>> GetActiveAsync(string? city = null, PropertyType? propertyType = null);

    /// <summary>
    /// Retrieves the active listings matching every supplied filter.
    /// </summary>
    /// <param name="criteria">The filters and ordering to apply.</param>
    /// <returns>The matching active listings with their images, amenities and reviews.</returns>
    Task<IEnumerable<HostProperty>> SearchAsync(PropertySearchCriteria criteria);

    /// <summary>
    /// Retrieves the distinct provinces that currently have active listings.
    /// </summary>
    /// <returns>The provinces, in enum order.</returns>
    Task<IEnumerable<Province>> GetProvincesAsync();

    /// <summary>
    /// Counts the active listings in each province, for the landing page's
    /// "browse by region" cards.
    /// </summary>
    /// <returns>A province-to-count map, covering only provinces that have listings.</returns>
    Task<IReadOnlyDictionary<Province, int>> GetProvinceCountsAsync();

    /// <summary>
    /// Retrieves the lowest and highest nightly rate across all active listings,
    /// used to bound the gallery's price filter.
    /// </summary>
    /// <returns>The minimum and maximum nightly rate in ZAR, or (0, 0) when there are none.</returns>
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync();

    /// <summary>
    /// Retrieves a listing with its images, amenities, reviews, and host.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <returns>The listing, or <c>null</c> when it does not exist.</returns>
    Task<HostProperty?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves every listing owned by the given host.
    /// </summary>
    /// <param name="hostId">The host's user identifier.</param>
    /// <returns>The host's listings.</returns>
    Task<IEnumerable<HostProperty>> GetByHostAsync(string hostId);

    /// <summary>
    /// Retrieves the distinct cities that currently have active listings.
    /// </summary>
    /// <returns>The city names, sorted alphabetically.</returns>
    Task<IEnumerable<string>> GetCitiesAsync();

    /// <summary>
    /// Persists a new listing.
    /// </summary>
    /// <param name="property">The listing to create.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(HostProperty property);

    /// <summary>
    /// Persists changes to an existing listing.
    /// </summary>
    /// <param name="property">The listing to update.</param>
    Task UpdateAsync(HostProperty property);

    /// <summary>
    /// Flips a listing's <see cref="HostProperty.IsActive"/> flag.
    /// </summary>
    /// <param name="id">The listing identifier.</param>
    /// <param name="hostId">The owning host, verified before the toggle is applied.</param>
    /// <returns><c>true</c> when the listing was toggled; otherwise <c>false</c>.</returns>
    Task<bool> ToggleActiveAsync(int id, string hostId);
}
