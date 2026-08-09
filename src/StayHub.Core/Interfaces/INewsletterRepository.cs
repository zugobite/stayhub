using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for newsletter subscriptions.
/// </summary>
public interface INewsletterRepository
{
    /// <summary>
    /// Returns whether the given email is already subscribed.
    /// </summary>
    /// <param name="email">The email to look up.</param>
    /// <returns><c>true</c> when a matching subscription exists.</returns>
    Task<bool> ExistsAsync(string email);

    /// <summary>
    /// Persists a new newsletter subscription.
    /// </summary>
    /// <param name="subscription">The subscription to store.</param>
    Task AddAsync(NewsletterSubscription subscription);
}
