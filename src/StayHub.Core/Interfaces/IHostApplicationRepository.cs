using StayHub.Core.Enums;
using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for host applications.
/// </summary>
public interface IHostApplicationRepository
{
    /// <summary>
    /// Retrieves an application with its applicant and proposed listing.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <returns>The application, or <c>null</c> when it does not exist.</returns>
    Task<HostApplication?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves applications for the administrator review queue.
    /// </summary>
    /// <param name="status">Optional status filter; omit for every application.</param>
    /// <returns>The matching applications, newest first.</returns>
    Task<IEnumerable<HostApplication>> GetAllAsync(ApplicationStatus? status = null);

    /// <summary>
    /// Retrieves every application submitted by a user.
    /// </summary>
    /// <param name="userId">The applicant's user identifier.</param>
    /// <returns>The user's applications, newest first.</returns>
    Task<IEnumerable<HostApplication>> GetByUserAsync(string userId);

    /// <summary>
    /// Persists a new application.
    /// </summary>
    /// <param name="application">The application to create.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(HostApplication application);

    /// <summary>
    /// Persists changes to an existing application.
    /// </summary>
    /// <param name="application">The application to update.</param>
    Task UpdateAsync(HostApplication application);
}
