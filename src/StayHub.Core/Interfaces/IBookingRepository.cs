using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for bookings.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Retrieves a booking with its property, guest, and payment.
    /// </summary>
    /// <param name="id">The booking identifier.</param>
    /// <returns>The booking, or <c>null</c> when it does not exist.</returns>
    Task<Booking?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves every booking made by a guest.
    /// </summary>
    /// <param name="guestId">The guest's user identifier.</param>
    /// <returns>The guest's bookings, newest first.</returns>
    Task<IEnumerable<Booking>> GetByGuestAsync(string guestId);

    /// <summary>
    /// Retrieves every booking placed against a host's listings.
    /// </summary>
    /// <param name="hostId">The host's user identifier.</param>
    /// <returns>The host's booking requests, newest first.</returns>
    Task<IEnumerable<Booking>> GetByHostAsync(string hostId);

    /// <summary>
    /// Determines whether a listing is free for the requested dates.
    /// </summary>
    /// <param name="propertyId">The listing identifier.</param>
    /// <param name="checkIn">The proposed arrival date.</param>
    /// <param name="checkOut">The proposed departure date.</param>
    /// <returns><c>true</c> when no confirmed booking overlaps the range.</returns>
    Task<bool> IsAvailableAsync(int propertyId, DateTime checkIn, DateTime checkOut);

    /// <summary>
    /// Persists a new booking request.
    /// </summary>
    /// <param name="booking">The booking to create.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(Booking booking);

    /// <summary>
    /// Persists changes to an existing booking.
    /// </summary>
    /// <param name="booking">The booking to update.</param>
    Task UpdateAsync(Booking booking);
}
