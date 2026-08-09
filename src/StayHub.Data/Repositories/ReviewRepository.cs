using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IReviewRepository"/>.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="ReviewRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public ReviewRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetByPropertyAsync(int propertyId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.PropertyId == propertyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Review>> GetByHostAsync(string hostId)
    {
        return await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Property)
            .Where(r => r.Property!.HostId == hostId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review.Id;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(int propertyId, string reviewerId)
    {
        return _context.Reviews.AnyAsync(r =>
            r.PropertyId == propertyId && r.ReviewerId == reviewerId);
    }
}
