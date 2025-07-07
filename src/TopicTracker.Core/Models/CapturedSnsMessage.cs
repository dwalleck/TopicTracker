using System;
using System.Collections.Generic;

namespace TopicTracker.Core.Models;

/// <summary>
/// Represents a captured SNS message with all metadata.
/// </summary>
public class CapturedSnsMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the captured message.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Gets or sets the timestamp when the message was captured.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
    
    /// <summary>
    /// Gets or sets the ARN of the topic the message was published to.
    /// </summary>
    public required string TopicArn { get; init; }
    
    /// <summary>
    /// Gets or sets the message body.
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Gets or sets the optional subject of the message.
    /// </summary>
    public string? Subject { get; init; }
    
    /// <summary>
    /// Gets or sets the message attributes.
    /// </summary>
    public Dictionary<string, MessageAttribute>? MessageAttributes { get; init; }
    
    /// <summary>
    /// Gets or sets the message structure (e.g., "json" for structured messages).
    /// </summary>
    public string? MessageStructure { get; init; }
    
    /// <summary>
    /// Gets or sets the message deduplication ID for FIFO topics.
    /// </summary>
    public string? MessageDeduplicationId { get; init; }
    
    /// <summary>
    /// Gets or sets the message group ID for FIFO topics.
    /// </summary>
    public string? MessageGroupId { get; init; }
    
    /// <summary>
    /// Gets or sets the raw payload as received from the SNS request.
    /// </summary>
    public required string RawPayload { get; init; }
}