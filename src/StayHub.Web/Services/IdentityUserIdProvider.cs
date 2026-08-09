using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace StayHub.Web.Services;

/// <summary>
/// Maps SignalR connections to the ASP.NET Core Identity user id so
/// <c>Clients.User(userId)</c> targets the correct browser sessions.
/// </summary>
public class IdentityUserIdProvider : IUserIdProvider
{
    /// <inheritdoc />
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
