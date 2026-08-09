using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="INewsletterRepository"/>.
/// </summary>
public class NewsletterRepository : INewsletterRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="NewsletterRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public NewsletterRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string email)
    {
        var normalised = email.Trim().ToLowerInvariant();
        return _context.NewsletterSubscriptions
            .AsNoTracking()
            .AnyAsync(s => s.Email == normalised);
    }

    /// <inheritdoc />
    public async Task AddAsync(NewsletterSubscription subscription)
    {
        subscription.Email = subscription.Email.Trim().ToLowerInvariant();
        _context.NewsletterSubscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }
}
