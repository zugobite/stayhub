using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for property reviews.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Retrieves the reviews of a listing.
    /// </summary>
    /// <param name="propertyId">The listing identifier.</param>
    /// <returns>The listing's reviews, newest first.</returns>
    Task<IEnumerable<Review>> GetByPropertyAsync(int propertyId);

    /// <summary>
    /// Retrieves the reviews across every listing a host owns.
    /// </summary>
    /// <param name="hostId">The host's user identifier.</param>
    /// <returns>The host's reviews, newest first.</returns>
    Task<IEnumerable<Review>> GetByHostAsync(string hostId);

    /// <summary>
    /// Persists a new review.
    /// </summary>
    /// <param name="review">The review to create.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(Review review);

    /// <summary>
    /// Determines whether the guest has already reviewed the listing.
    /// </summary>
    /// <param name="propertyId">The listing identifier.</param>
    /// <param name="reviewerId">The guest's user identifier.</param>
    /// <returns><c>true</c> when a review already exists.</returns>
    Task<bool> ExistsAsync(int propertyId, string reviewerId);
}
