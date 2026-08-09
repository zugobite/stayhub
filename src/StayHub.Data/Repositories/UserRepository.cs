using Microsoft.EntityFrameworkCore;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public UserRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApplicationUser>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return await GetAllAsync();

        return await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.FirstName.Contains(term) ||
                u.LastName.Contains(term) ||
                u.Email!.Contains(term))
            .OrderBy(u => u.FirstName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CountAsync() => await _context.Users.CountAsync();
}
