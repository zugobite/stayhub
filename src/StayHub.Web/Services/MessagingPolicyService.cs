using Microsoft.AspNetCore.Identity;
using StayHub.Core.Constants;
using StayHub.Core.Interfaces;
using StayHub.Core.Models;

namespace StayHub.Web.Services;

/// <summary>
/// Guest-host messaging rules: staff may message anyone and anyone may reply to
/// staff; guests and hosts may message each other when a booking links them or
/// the recipient hosts a listing (property-page inquiry).
/// </summary>
public class MessagingPolicyService : IMessagingPolicyService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookingRepository _bookings;
    private readonly IPropertyRepository _properties;
    private readonly IMessageRepository _messages;

    /// <summary>
    /// Initialises a new instance of the <see cref="MessagingPolicyService"/> class.
    /// </summary>
    public MessagingPolicyService(
        UserManager<ApplicationUser> userManager,
        IBookingRepository bookings,
        IPropertyRepository properties,
        IMessageRepository messages)
    {
        _userManager = userManager;
        _bookings = bookings;
        _properties = properties;
        _messages = messages;
    }

    /// <inheritdoc />
    public async Task<bool> CanMessageAsync(string senderId, string receiverId)
    {
        if (string.IsNullOrWhiteSpace(senderId) ||
            string.IsNullOrWhiteSpace(receiverId) ||
            string.Equals(senderId, receiverId, StringComparison.Ordinal))
        {
            return false;
        }

        var sender = await _userManager.FindByIdAsync(senderId);
        var receiver = await _userManager.FindByIdAsync(receiverId);

        if (sender is null || receiver is null)
            return false;

        // Staff can reach anyone; hosts/guests must be able to open and reply to
        // those threads (otherwise admin → host messages are invisible in practice).
        if (await IsStaffAsync(sender) || await IsStaffAsync(receiver))
            return true;

        // Once a conversation exists (e.g. admin wrote first), either party may continue it.
        if (await _messages.ConversationExistsAsync(senderId, receiverId))
            return true;

        if (await HasBookingRelationshipAsync(senderId, receiverId))
            return true;

        // Property-page inquiry: any signed-in user may message a host who
        // currently owns at least one listing.
        if (await _userManager.IsInRoleAsync(receiver, Roles.Host))
        {
            var listings = await _properties.GetByHostAsync(receiverId);
            if (listings.Any())
                return true;
        }

        return false;
    }

    private async Task<bool> IsStaffAsync(ApplicationUser user) =>
        await _userManager.IsInRoleAsync(user, Roles.Admin) ||
        await _userManager.IsInRoleAsync(user, Roles.SuperAdmin);

    private async Task<bool> HasBookingRelationshipAsync(string userA, string userB)
    {
        var asGuest = await _bookings.GetByGuestAsync(userA);
        if (asGuest.Any(b => b.Property?.HostId == userB))
            return true;

        var asHost = await _bookings.GetByHostAsync(userA);
        if (asHost.Any(b => b.GuestId == userB))
            return true;

        return false;
    }
}
