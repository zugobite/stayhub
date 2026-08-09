using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ICheckInRepository"/>.
/// </summary>
public class CheckInRepository : ICheckInRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="CheckInRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public CheckInRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<CheckInProcess?> GetProcessByPropertyAsync(int propertyId)
    {
        return await _context.CheckInProcesses
            .FirstOrDefaultAsync(c => c.PropertyId == propertyId);
    }

    /// <inheritdoc />
    public async Task<int> SaveProcessAsync(CheckInProcess process)
    {
        var existing = await _context.CheckInProcesses
            .FirstOrDefaultAsync(c => c.PropertyId == process.PropertyId);

        if (existing is null)
        {
            _context.CheckInProcesses.Add(process);
            await _context.SaveChangesAsync();
            return process.Id;
        }

        existing.Title = process.Title;
        existing.StepsJson = process.StepsJson;
        await _context.SaveChangesAsync();
        return existing.Id;
    }

    /// <inheritdoc />
    public async Task<GuestCheckIn?> GetCheckInByBookingAsync(int bookingId)
    {
        return await _context.GuestCheckIns
            .Include(c => c.Documents)
            .Include(c => c.CheckInProcess)
            .Include(c => c.Booking)
                .ThenInclude(b => b!.Property)
            .FirstOrDefaultAsync(c => c.BookingId == bookingId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GuestCheckIn>> GetPendingVerificationAsync(string hostId)
    {
        return await _context.GuestCheckIns
            .Include(c => c.Documents)
            .Include(c => c.Booking)
                .ThenInclude(b => b!.Guest)
            .Include(c => c.Booking)
                .ThenInclude(b => b!.Property)
            .Where(c => c.Booking!.Property!.HostId == hostId &&
                        c.Status == CheckInStatus.PendingVerification)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> CreateCheckInAsync(GuestCheckIn checkIn)
    {
        _context.GuestCheckIns.Add(checkIn);
        await _context.SaveChangesAsync();
        return checkIn.Id;
    }

    /// <inheritdoc />
    public async Task UpdateCheckInAsync(GuestCheckIn checkIn)
    {
        _context.GuestCheckIns.Update(checkIn);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<int> AddDocumentAsync(GuestDocument document)
    {
        _context.GuestDocuments.Add(document);
        await _context.SaveChangesAsync();
        return document.Id;
    }

    /// <inheritdoc />
    public async Task<bool> VerifyDocumentAsync(int documentId, bool approved)
    {
        var document = await _context.GuestDocuments
            .Include(d => d.GuestCheckIn)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document is null)
            return false;

        document.Status = approved ? DocumentStatus.Verified : DocumentStatus.Rejected;

        if (document.GuestCheckIn is not null)
        {
            document.GuestCheckIn.Status = approved
                ? CheckInStatus.Verified
                : CheckInStatus.Rejected;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
