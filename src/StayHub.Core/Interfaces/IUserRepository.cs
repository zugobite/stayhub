using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the read-side data access contract for users. Credential handling
/// stays with ASP.NET Core Identity; this contract serves the admin screens.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <returns>The user, or <c>null</c> when they do not exist.</returns>
    Task<ApplicationUser?> GetByIdAsync(string id);

    /// <summary>
    /// Retrieves every registered user for the administrator's user table.
    /// </summary>
    /// <returns>The users, newest registration first.</returns>
    Task<IEnumerable<ApplicationUser>> GetAllAsync();

    /// <summary>
    /// Searches users by name or email address.
    /// </summary>
    /// <param name="term">The search term.</param>
    /// <returns>The matching users.</returns>
    Task<IEnumerable<ApplicationUser>> SearchAsync(string term);

    /// <summary>
    /// Counts the registered users.
    /// </summary>
    /// <returns>The total user count.</returns>
    Task<int> CountAsync();
}
