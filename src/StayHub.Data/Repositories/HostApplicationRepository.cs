using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IHostApplicationRepository"/>.
/// </summary>
public class HostApplicationRepository : IHostApplicationRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="HostApplicationRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public HostApplicationRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<HostApplication?> GetByIdAsync(int id)
    {
        return await _context.HostApplications
            .Include(a => a.ApplicationUser)
            .Include(a => a.Property)
                .ThenInclude(p => p!.Images)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HostApplication>> GetAllAsync(ApplicationStatus? status = null)
    {
        var query = _context.HostApplications
            .Include(a => a.ApplicationUser)
            .Include(a => a.Property)
                .ThenInclude(p => p!.Images)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query.OrderByDescending(a => a.AppliedAt).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HostApplication>> GetByUserAsync(string userId)
    {
        return await _context.HostApplications
            .Include(a => a.Property)
                .ThenInclude(p => p!.Images)
            .Where(a => a.ApplicationUserId == userId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(HostApplication application)
    {
        _context.HostApplications.Add(application);
        await _context.SaveChangesAsync();
        return application.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(HostApplication application)
    {
        _context.HostApplications.Update(application);
        await _context.SaveChangesAsync();
    }
}
