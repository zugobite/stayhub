using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for the check-in workflow: the host's
/// process definition, the guest's run through it, and the uploaded documents.
/// </summary>
public interface ICheckInRepository
{
    /// <summary>
    /// Retrieves the check-in process a host defined for a listing.
    /// </summary>
    /// <param name="propertyId">The listing identifier.</param>
    /// <returns>The process, or <c>null</c> when the host has not defined one.</returns>
    Task<CheckInProcess?> GetProcessByPropertyAsync(int propertyId);

    /// <summary>
    /// Creates or replaces a listing's check-in process.
    /// </summary>
    /// <param name="process">The process definition.</param>
    /// <returns>The process identifier.</returns>
    Task<int> SaveProcessAsync(CheckInProcess process);

    /// <summary>
    /// Retrieves the check-in raised for a booking, with its documents.
    /// </summary>
    /// <param name="bookingId">The booking identifier.</param>
    /// <returns>The check-in, or <c>null</c> when the guest has not started one.</returns>
    Task<GuestCheckIn?> GetCheckInByBookingAsync(int bookingId);

    /// <summary>
    /// Retrieves the check-ins awaiting document verification across a host's listings.
    /// </summary>
    /// <param name="hostId">The host's user identifier.</param>
    /// <returns>The pending check-ins.</returns>
    Task<IEnumerable<GuestCheckIn>> GetPendingVerificationAsync(string hostId);

    /// <summary>
    /// Persists a new guest check-in.
    /// </summary>
    /// <param name="checkIn">The check-in to create.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateCheckInAsync(GuestCheckIn checkIn);

    /// <summary>
    /// Persists changes to an existing guest check-in.
    /// </summary>
    /// <param name="checkIn">The check-in to update.</param>
    Task UpdateCheckInAsync(GuestCheckIn checkIn);

    /// <summary>
    /// Attaches an uploaded identity document to a check-in.
    /// </summary>
    /// <param name="document">The document to store.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> AddDocumentAsync(GuestDocument document);

    /// <summary>
    /// Records a host's verification decision on a document.
    /// </summary>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="approved">Whether the host accepted the document.</param>
    /// <returns><c>true</c> when the decision was recorded; otherwise <c>false</c>.</returns>
    Task<bool> VerifyDocumentAsync(int documentId, bool approved);
}
