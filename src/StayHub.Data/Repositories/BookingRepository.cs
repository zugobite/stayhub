using Microsoft.EntityFrameworkCore;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IBookingRepository"/>.
/// </summary>
public class BookingRepository : IBookingRepository
{
    private readonly StayHubDbContext _context;

    /// <summary>
    /// Initialises a new instance of the <see cref="BookingRepository"/> class.
    /// </summary>
    /// <param name="context">The EF Core context.</param>
    public BookingRepository(StayHubDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Property)
                .ThenInclude(p => p!.Images)
            .Include(b => b.Guest)
            .Include(b => b.Payment)
            .Include(b => b.GuestCheckIn)
                .ThenInclude(c => c!.Documents)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Booking>> GetByGuestAsync(string guestId)
    {
        return await _context.Bookings
            .Include(b => b.Property)
                .ThenInclude(p => p!.Images)
            .Include(b => b.GuestCheckIn)
            .Where(b => b.GuestId == guestId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Booking>> GetByHostAsync(string hostId)
    {
        return await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .Include(b => b.GuestCheckIn)
                .ThenInclude(c => c!.Documents)
            .Where(b => b.Property!.HostId == hostId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(int propertyId, DateTime checkIn, DateTime checkOut)
    {
        var blocking = new[]
        {
            BookingStatus.Pending,
            BookingStatus.Approved,
            BookingStatus.Paid,
            BookingStatus.CheckedIn
        };

        return !await _context.Bookings.AnyAsync(b =>
            b.PropertyId == propertyId &&
            blocking.Contains(b.Status) &&
            b.CheckInDate < checkOut &&
            checkIn < b.CheckOutDate);
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }
}
