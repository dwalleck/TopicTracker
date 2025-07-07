using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tethys.Results;
using TopicTracker.Core.Models;

namespace TopicTracker.Core.Storage;

/// <summary>
/// Thread-safe in-memory implementation of IMessageStore with LRU eviction.
/// </summary>
public class InMemoryMessageStore : IMessageStore
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly LinkedList<CapturedSnsMessage> _messages = new();
    private readonly Dictionary<string, LinkedListNode<CapturedSnsMessage>> _messageIndex = new();
    private readonly Dictionary<string, List<LinkedListNode<CapturedSnsMessage>>> _topicIndex = new();
    private readonly int _maxMessages;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMessageStore"/> class.
    /// </summary>
    /// <param name="maxMessages">The maximum number of messages to store. Defaults to 1000.</param>
    public InMemoryMessageStore(int maxMessages = 1000)
    {
        if (maxMessages <= 0)
        {
            throw new ArgumentException("Max messages must be greater than 0", nameof(maxMessages));
        }
        _maxMessages = maxMessages;
    }

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _messages.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    /// <inheritdoc/>
    public int MaxMessages => _maxMessages;

    /// <inheritdoc/>
    public Result Add(CapturedSnsMessage message)
    {
        if (message == null)
        {
            var error = new MessageStoreError("NULL_MESSAGE", "Message cannot be null");
            return Result.Fail(error.Message, error);
        }

        _lock.EnterWriteLock();
        try
        {
            // Check if message with same ID already exists
            if (_messageIndex.TryGetValue(message.Id, out var existingNode))
            {
                // Remove the existing message first
                RemoveNodeFromIndexes(existingNode);
                _messages.Remove(existingNode);
            }

            // If we're at capacity, remove the oldest message (LRU eviction)
            if (_messages.Count >= _maxMessages)
            {
                var oldest = _messages.First;
                if (oldest != null)
                {
                    RemoveNodeFromIndexes(oldest);
                    _messages.RemoveFirst();
                }
            }

            // Add the new message
            var node = _messages.AddLast(message);
            
            // Update indexes
            _messageIndex[message.Id] = node;
            
            if (!_topicIndex.TryGetValue(message.TopicArn, out var topicList))
            {
                topicList = new List<LinkedListNode<CapturedSnsMessage>>();
                _topicIndex[message.TopicArn] = topicList;
            }
            topicList.Add(node);

            return Result.Ok("Message stored successfully");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public Result<IReadOnlyList<CapturedSnsMessage>> GetMessages()
    {
        _lock.EnterReadLock();
        try
        {
            var messages = _messages.ToList();
            return Result<IReadOnlyList<CapturedSnsMessage>>.Ok(messages, "Messages retrieved successfully");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Result<IReadOnlyList<CapturedSnsMessage>> GetMessagesByTopic(string topicArn)
    {
        if (string.IsNullOrEmpty(topicArn))
        {
            var error = new MessageStoreError("NULL_TOPIC_ARN", "Topic ARN cannot be null or empty");
            return Result<IReadOnlyList<CapturedSnsMessage>>.Fail(error.Message, error);
        }

        _lock.EnterReadLock();
        try
        {
            if (_topicIndex.TryGetValue(topicArn, out var nodes))
            {
                var messages = nodes
                    .Where(n => n.List != null)
                    .Select(n => n.Value)
                    .ToList();
                return Result<IReadOnlyList<CapturedSnsMessage>>.Ok(messages, "Messages retrieved successfully");
            }
            
            return Result<IReadOnlyList<CapturedSnsMessage>>.Ok(
                new List<CapturedSnsMessage>(), "No messages found for topic");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Result<IReadOnlyList<CapturedSnsMessage>> GetMessagesByTimeRange(DateTimeOffset startTime, DateTimeOffset endTime)
    {
        _lock.EnterReadLock();
        try
        {
            var messages = _messages
                .Where(m => m.Timestamp >= startTime && m.Timestamp <= endTime)
                .ToList();
            return Result<IReadOnlyList<CapturedSnsMessage>>.Ok(messages, "Messages retrieved successfully");
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Result<CapturedSnsMessage> GetMessageById(string messageId)
    {
        if (string.IsNullOrEmpty(messageId))
        {
            var error = new MessageStoreError("NULL_MESSAGE_ID", "Message ID cannot be null or empty");
            return Result<CapturedSnsMessage>.Fail(error.Message, error);
        }

        _lock.EnterReadLock();
        try
        {
            if (_messageIndex.TryGetValue(messageId, out var node) && node.List != null)
            {
                return Result<CapturedSnsMessage>.Ok(node.Value, "Message found");
            }
            
            var error = new MessageStoreError("MESSAGE_NOT_FOUND", $"Message with ID '{messageId}' not found");
            return Result<CapturedSnsMessage>.Fail(error.Message, error);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public Result Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _messages.Clear();
            _messageIndex.Clear();
            _topicIndex.Clear();
            return Result.Ok("All messages cleared");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void RemoveNodeFromIndexes(LinkedListNode<CapturedSnsMessage> node)
    {
        var message = node.Value;
        
        // Remove from message index
        _messageIndex.Remove(message.Id);
        
        // Remove from topic index
        if (_topicIndex.TryGetValue(message.TopicArn, out var topicList))
        {
            topicList.Remove(node);
            if (topicList.Count == 0)
            {
                _topicIndex.Remove(message.TopicArn);
            }
        }
    }

    /// <summary>
    /// Disposes the reader-writer lock.
    /// </summary>
    public void Dispose()
    {
        _lock?.Dispose();
    }
}