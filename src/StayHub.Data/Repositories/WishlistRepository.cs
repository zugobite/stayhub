using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IWishlistRepository"/>.
/// </summary>
public class WishlistRepository : IWishlistRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="WishlistRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public WishlistRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<WishlistItem>> GetByUserAsync(string userId)
    {
        return await _context.WishlistItems
            .Include(w => w.Property)
                .ThenInclude(p => p!.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> ToggleAsync(string userId, int propertyId)
    {
        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PropertyId == propertyId);

        if (existing is not null)
        {
            _context.WishlistItems.Remove(existing);
            await _context.SaveChangesAsync();
            return false;
        }

        _context.WishlistItems.Add(new WishlistItem
        {
            UserId = userId,
            PropertyId = propertyId,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }
}
