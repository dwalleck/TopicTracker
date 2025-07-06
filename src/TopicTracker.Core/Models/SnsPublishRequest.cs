using System.Collections.Generic;

namespace TopicTracker.Core.Models;

/// <summary>
/// Represents an SNS publish request matching the AWS SDK structure.
/// </summary>
public class SnsPublishRequest
{
    /// <summary>
    /// Gets or sets the ARN of the topic to publish to.
    /// </summary>
    public string? TopicArn { get; init; }
    
    /// <summary>
    /// Gets or sets the ARN of the target endpoint (for direct publish).
    /// </summary>
    public string? TargetArn { get; init; }
    
    /// <summary>
    /// Gets or sets the phone number to send SMS messages to.
    /// </summary>
    public string? PhoneNumber { get; init; }
    
    /// <summary>
    /// Gets or sets the message to be sent.
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// Gets or sets the optional subject for the message.
    /// </summary>
    public string? Subject { get; init; }
    
    /// <summary>
    /// Gets or sets the structure of the message (e.g., "json").
    /// </summary>
    public string? MessageStructure { get; init; }
    
    /// <summary>
    /// Gets or sets the message attributes.
    /// </summary>
    public Dictionary<string, MessageAttribute>? MessageAttributes { get; init; }
    
    /// <summary>
    /// Gets or sets the message deduplication ID for FIFO topics.
    /// </summary>
    public string? MessageDeduplicationId { get; init; }
    
    /// <summary>
    /// Gets or sets the message group ID for FIFO topics.
    /// </summary>
    public string? MessageGroupId { get; init; }
}