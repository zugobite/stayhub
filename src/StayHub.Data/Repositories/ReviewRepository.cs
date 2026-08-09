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
    public async Task<IEnumerable<Review>> GetRecentHighlightsAsync(int count, int minimumRating = 4)
    {
        var candidates = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Reviewer)
            .Include(r => r.Property)
            // Only surface reviews whose listing is still published, so a
            // testimonial never links to something a guest cannot book.
            .Where(r => r.Rating >= minimumRating && r.Property!.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count * 6)
            .ToListAsync();

        // Taking the newest N outright can surface the same guest three times,
        // which reads as fabricated. Prefer one review per guest, then per
        // listing, and only repeat when there is nothing else to show.
        var picked = new List<Review>(count);
        var seenReviewers = new HashSet<string>(StringComparer.Ordinal);
        var seenProperties = new HashSet<int>();

        foreach (var review in candidates)
        {
            if (picked.Count == count)
                break;

            if (seenReviewers.Add(review.ReviewerId) && seenProperties.Add(review.PropertyId))
                picked.Add(review);
        }

        foreach (var review in candidates)
        {
            if (picked.Count == count)
                break;

            if (!picked.Contains(review) && seenProperties.Add(review.PropertyId))
                picked.Add(review);
        }

        foreach (var review in candidates)
        {
            if (picked.Count == count)
                break;

            if (!picked.Contains(review))
                picked.Add(review);
        }

        return picked;
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
