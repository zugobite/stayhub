using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for saved listings.
/// </summary>
public interface IWishlistRepository
{
    /// <summary>
    /// Retrieves the listings a user has saved.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <returns>The saved listings, newest first.</returns>
    Task<IEnumerable<WishlistItem>> GetByUserAsync(string userId);

    /// <summary>
    /// Adds or removes a listing from a user's wishlist.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <param name="propertyId">The listing identifier.</param>
    /// <returns><c>true</c> when the listing is now saved; <c>false</c> when it was removed.</returns>
    Task<bool> ToggleAsync(string userId, int propertyId);
}
