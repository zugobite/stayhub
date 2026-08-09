using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAmenityRepository"/>.
/// </summary>
public class AmenityRepository : IAmenityRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="AmenityRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public AmenityRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Amenity>> GetAllAsync()
    {
        return await _context.Amenities
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
}
