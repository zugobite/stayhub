using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPropertyRepository"/>.
/// </summary>
public class PropertyRepository : IPropertyRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="PropertyRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public PropertyRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HostProperty>> GetActiveAsync(string? city = null, PropertyType? propertyType = null)
    {
        var query = _context.Properties
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => p.City == city);

        if (propertyType.HasValue)
            query = query.Where(p => p.PropertyType == propertyType.Value);

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HostProperty>> SearchAsync(PropertySearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        var query = _context.Properties
            .AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var term = criteria.Keyword.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Title, $"%{term}%")
                || EF.Functions.Like(p.Description, $"%{term}%")
                || EF.Functions.Like(p.City, $"%{term}%")
                || EF.Functions.Like(p.Address, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(criteria.City))
            query = query.Where(p => p.City == criteria.City);

        if (criteria.Province.HasValue)
            query = query.Where(p => p.Province == criteria.Province.Value);

        if (criteria.PropertyType.HasValue)
            query = query.Where(p => p.PropertyType == criteria.PropertyType.Value);

        if (criteria.MinPrice.HasValue)
            query = query.Where(p => p.PricePerNight >= criteria.MinPrice.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(p => p.PricePerNight <= criteria.MaxPrice.Value);

        if (criteria.Guests.HasValue)
            query = query.Where(p => p.MaxGuests >= criteria.Guests.Value);

        if (criteria.Bedrooms.HasValue)
            query = query.Where(p => p.Bedrooms >= criteria.Bedrooms.Value);

        if (criteria.Bathrooms.HasValue)
            query = query.Where(p => p.Bathrooms >= criteria.Bathrooms.Value);

        // Every requested amenity must be present, so the filters narrow rather
        // than widen as more boxes are ticked.
        foreach (var amenityId in criteria.AmenityIds.Distinct())
        {
            var id = amenityId;
            query = query.Where(p => p.PropertyAmenities.Any(pa => pa.AmenityId == id));
        }

        query = criteria.SortOrder switch
        {
            PropertySortOrder.PriceLowToHigh => query.OrderBy(p => p.PricePerNight).ThenBy(p => p.Id),
            PropertySortOrder.PriceHighToLow => query.OrderByDescending(p => p.PricePerNight).ThenBy(p => p.Id),
            // Listings with no reviews sort last rather than being treated as zero-rated.
            PropertySortOrder.TopRated => query
                .OrderByDescending(p => p.Reviews.Any())
                .ThenByDescending(p => p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0d)
                .ThenBy(p => p.Id),
            PropertySortOrder.Capacity => query.OrderByDescending(p => p.MaxGuests).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id)
        };

        return await query.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Province>> GetProvincesAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => p.Province)
            .Distinct()
            .OrderBy(p => p)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync()
    {
        var active = _context.Properties.AsNoTracking().Where(p => p.IsActive);

        if (!await active.AnyAsync())
            return (0m, 0m);

        return (await active.MinAsync(p => p.PricePerNight),
                await active.MaxAsync(p => p.PricePerNight));
    }

    /// <inheritdoc />
    public async Task<HostProperty?> GetByIdAsync(int id)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.Host)
            .Include(p => p.CheckInProcess)
            .Include(p => p.PropertyAmenities)
                .ThenInclude(pa => pa.Amenity)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.Reviewer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HostProperty>> GetByHostAsync(string hostId)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.Bookings)
            .Include(p => p.Reviews)
            .Where(p => p.HostId == hostId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCitiesAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .Where(p => p.IsActive)
            .Select(p => p.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(HostProperty property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(HostProperty property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ToggleActiveAsync(int id, string hostId)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == hostId);

        if (property is null)
            return false;

        if (property.IsActive)
            property.Deactivate();
        else
            property.Activate();

        await _context.SaveChangesAsync();
        return true;
    }
}
