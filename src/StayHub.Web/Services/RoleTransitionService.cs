using Microsoft.AspNetCore.Identity;
using StayHub.Core.Constants;
using StayHub.Core.Enums;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Web.Services;

/// <summary>
/// Identity-backed implementation of <see cref="IRoleTransitionService"/>.
/// </summary>
public class RoleTransitionService : IRoleTransitionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogRepository _activityLog;

    /// <summary>
    /// Initialises a new instance of the <see cref="RoleTransitionService"/> class.
    /// </summary>
    /// <param name="userManager">The Identity user manager.</param>
    /// <param name="activityLog">The audit trail repository.</param>
    public RoleTransitionService(
        UserManager<ApplicationUser> userManager,
        IActivityLogRepository activityLog)
    {
        _userManager = userManager;
        _activityLog = activityLog;
    }

    /// <inheritdoc />
    public async Task<bool> PromoteGuestToHostAsync(ApplicationUser user, string actingUserId)
    {
        if (await _userManager.IsInRoleAsync(user, Roles.Host))
            return true; // Already a host: an additional-property application.

        var added = await _userManager.AddToRoleAsync(user, Roles.Host);
        if (!added.Succeeded)
            return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
            await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

        await _activityLog.LogAsync(
            actingUserId,
            "RoleChangedGuestToHost",
            ActivityType.RoleChange,
            $"{user.Email} promoted to Host; Guest role removed.");

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> PromoteToAdminAsync(ApplicationUser user, string actingUserId)
    {
        if (await _userManager.IsInRoleAsync(user, Roles.Admin))
            return true;

        var added = await _userManager.AddToRoleAsync(user, Roles.Admin);
        if (!added.Succeeded)
            return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
            await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

        await _activityLog.LogAsync(
            actingUserId,
            "RoleChangedGuestToAdmin",
            ActivityType.RoleChange,
            $"{user.Email} promoted to Admin by SuperAdmin.");

        return true;
    }
}
