using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StayHub.Web.Hubs;

/// <summary>
/// Authenticated SignalR hub used to push live messages and notification
/// badge updates to signed-in users.
/// </summary>
[Authorize]
public class StayHubHub : Hub
{
}
