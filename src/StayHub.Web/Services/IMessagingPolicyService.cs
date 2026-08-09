namespace StayHub.Web.Services;

/// <summary>
/// Decides whether one user may open or continue a conversation with another.
/// </summary>
public interface IMessagingPolicyService
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="senderId"/> is allowed to message
    /// <paramref name="receiverId"/>.
    /// </summary>
    Task<bool> CanMessageAsync(string senderId, string receiverId);
}
