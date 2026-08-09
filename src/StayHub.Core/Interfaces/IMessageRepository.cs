using StayHub.Core.Models;

namespace StayHub.Core.Interfaces;

/// <summary>
/// Defines the data access contract for the database-backed inbox.
/// </summary>
public interface IMessageRepository
{
    /// <summary>
    /// Retrieves the messages a user has received.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <returns>The received messages, newest first.</returns>
    Task<IEnumerable<Message>> GetInboxAsync(string userId);

    /// <summary>
    /// Retrieves the messages a user has sent.
    /// </summary>
    /// <param name="userId">The sender's user identifier.</param>
    /// <returns>The sent messages, newest first.</returns>
    Task<IEnumerable<Message>> GetSentAsync(string userId);

    /// <summary>
    /// Retrieves one conversation preview per correspondent.
    /// </summary>
    /// <param name="userId">The signed-in user's identifier.</param>
    /// <returns>The threads, newest activity first.</returns>
    Task<IEnumerable<MessageThread>> GetThreadsAsync(string userId);

    /// <summary>
    /// Retrieves the full exchange between two users.
    /// </summary>
    /// <param name="userId">The signed-in user's identifier.</param>
    /// <param name="otherUserId">The correspondent's identifier.</param>
    /// <returns>The conversation, oldest first.</returns>
    Task<IEnumerable<Message>> GetConversationAsync(string userId, string otherUserId);

    /// <summary>
    /// Counts a user's unread messages for the header badge.
    /// </summary>
    /// <param name="userId">The recipient's user identifier.</param>
    /// <returns>The unread count.</returns>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>
    /// Persists a new message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>The generated identifier.</returns>
    Task<int> CreateAsync(Message message);

    /// <summary>
    /// Marks a received message as read.
    /// </summary>
    /// <param name="id">The message identifier.</param>
    /// <param name="userId">The recipient, verified before the flag is written.</param>
    /// <returns><c>true</c> when the flag was written; otherwise <c>false</c>.</returns>
    Task<bool> MarkAsReadAsync(int id, string userId);
}
