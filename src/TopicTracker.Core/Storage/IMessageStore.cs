using TopicTracker.Core.Models;
using Tethys.Results;

namespace TopicTracker.Core.Storage;

/// <summary>
/// Defines the contract for storing and retrieving captured SNS messages.
/// </summary>
public interface IMessageStore : IDisposable
{
    /// <summary>
    /// Adds a captured message to the store.
    /// </summary>
    /// <param name="message">The message to store.</param>
    /// <returns>A Result indicating success or failure with error details.</returns>
    Result Add(CapturedSnsMessage message);

    /// <summary>
    /// Gets all messages from the store.
    /// </summary>
    /// <returns>A Result containing all messages or error details.</returns>
    Result<IReadOnlyList<CapturedSnsMessage>> GetMessages();

    /// <summary>
    /// Gets messages filtered by topic ARN.
    /// </summary>
    /// <param name="topicArn">The topic ARN to filter by.</param>
    /// <returns>A Result containing filtered messages or error details.</returns>
    Result<IReadOnlyList<CapturedSnsMessage>> GetMessagesByTopic(string topicArn);

    /// <summary>
    /// Gets messages within a specific time range.
    /// </summary>
    /// <param name="startTime">The start time (inclusive).</param>
    /// <param name="endTime">The end time (inclusive).</param>
    /// <returns>A Result containing filtered messages or error details.</returns>
    Result<IReadOnlyList<CapturedSnsMessage>> GetMessagesByTimeRange(DateTimeOffset startTime, DateTimeOffset endTime);

    /// <summary>
    /// Gets a specific message by its ID.
    /// </summary>
    /// <param name="messageId">The message ID to search for.</param>
    /// <returns>A Result containing the message or error details.</returns>
    Result<CapturedSnsMessage> GetMessageById(string messageId);

    /// <summary>
    /// Clears all messages from the store.
    /// </summary>
    /// <returns>A Result indicating success or failure.</returns>
    Result Clear();

    /// <summary>
    /// Gets the current message count in the store.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the maximum number of messages that can be stored.
    /// </summary>
    int MaxMessages { get; }
}