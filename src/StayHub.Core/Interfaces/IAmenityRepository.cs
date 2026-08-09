using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for the amenity lookup table.
/// </summary>
public interface IAmenityRepository
{
    /// <summary>
    /// Retrieves every amenity, for the gallery's filter checkboxes and the
    /// host application form.
    /// </summary>
    /// <returns>The amenities, sorted by name.</returns>
    Task<IEnumerable<Amenity>> GetAllAsync();
}
