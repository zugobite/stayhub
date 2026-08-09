using StayHub.Core.Models;

namespace StayHub.Web.Services;

/// <summary>
/// Owns the role changes the brief calls for, so the swap logic lives in one
/// place rather than being repeated across controllers.
/// </summary>
public interface IRoleTransitionService
{
    /// <summary>
    /// Promotes a Guest to Host: the Host role is added and the Guest role is
    /// removed, because a user is either a Guest or a Host and never both.
    /// The change is written to the activity log.
    /// </summary>
    /// <param name="user">The user being promoted.</param>
    /// <param name="actingUserId">The administrator performing the promotion.</param>
    /// <returns><c>true</c> when the swap succeeded; otherwise <c>false</c>.</returns>
    Task<bool> PromoteGuestToHostAsync(ApplicationUser user, string actingUserId);

    /// <summary>
    /// Promotes a Guest to Admin. Only a SuperAdmin may call this.
    /// The change is written to the activity log.
    /// </summary>
    /// <param name="user">The user being promoted.</param>
    /// <param name="actingUserId">The SuperAdmin performing the promotion.</param>
    /// <returns><c>true</c> when the promotion succeeded; otherwise <c>false</c>.</returns>
    Task<bool> PromoteToAdminAsync(ApplicationUser user, string actingUserId);
}
