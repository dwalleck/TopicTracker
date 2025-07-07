using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Models;
using TopicTracker.Core.Storage;

namespace TopicTracker.Core.Tests.Storage;

public class InMemoryMessageStoreTests
{
    private InMemoryMessageStore _store = null!;
    
    [Before(Test)]
    public void Setup()
    {
        _store = new InMemoryMessageStore(maxMessages: 100);
    }
    
    [Test]
    public async Task Add_Should_Store_Message_Successfully()
    {
        // Arrange
        var message = CreateTestMessage("msg-1");
        
        // Act
        var result = _store.Add(message);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(_store.Count).IsEqualTo(1);
    }
    
    [Test]
    public async Task Add_Should_Return_Failure_For_Null_Message()
    {
        // Act
        var result = _store.Add(null!);
        
        // Assert
        await Assert.That(!result.Success).IsTrue();
        await Assert.That(result.Exception).IsNotNull();
        await Assert.That(result.Exception).IsTypeOf<MessageStoreError>();
        var error = (MessageStoreError)result.Exception!;
        await Assert.That(error.Code).IsEqualTo("NULL_MESSAGE");
    }
    
    [Test]
    public async Task Add_Should_Enforce_Max_Message_Limit_With_LRU_Eviction()
    {
        // Arrange
        var store = new InMemoryMessageStore(maxMessages: 3);
        var messages = Enumerable.Range(1, 5).Select(i => CreateTestMessage($"msg-{i}")).ToList();
        
        // Act - Add 5 messages to a store with max 3
        foreach (var message in messages)
        {
            var result = store.Add(message);
            await Assert.That(result.Success).IsTrue();
        }
        
        // Assert - Should have only the last 3 messages
        await Assert.That(store.Count).IsEqualTo(3);
        var storedMessages = store.GetMessages();
        await Assert.That(storedMessages.Success).IsTrue();
        await Assert.That(storedMessages.Data!.Select(m => m.Id))
            .IsEquivalentTo(new[] { "msg-3", "msg-4", "msg-5" });
    }
    
    [Test]
    public async Task Add_Should_Have_Low_Latency()
    {
        // Arrange
        var message = CreateTestMessage("perf-test");
        var stopwatch = new Stopwatch();
        
        // Act
        stopwatch.Start();
        var result = _store.Add(message);
        stopwatch.Stop();
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(stopwatch.Elapsed.TotalMicroseconds).IsLessThan(100); // <100μs requirement
    }
    
    [Test]
    public async Task GetMessages_Should_Return_All_Messages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 3).Select(i => CreateTestMessage($"msg-{i}")).ToList();
        foreach (var message in messages)
        {
            _store.Add(message);
        }
        
        // Act
        var result = _store.GetMessages();
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data!.Count).IsEqualTo(3);
        await Assert.That(result.Data!.Select(m => m.Id))
            .IsEquivalentTo(messages.Select(m => m.Id));
    }
    
    [Test]
    public async Task GetMessagesByTopic_Should_Filter_Correctly()
    {
        // Arrange
        var topic1Messages = Enumerable.Range(1, 2).Select(i => CreateTestMessage($"msg-{i}", "arn:aws:sns:us-east-1:123456789012:topic1")).ToList();
        var topic2Messages = Enumerable.Range(3, 2).Select(i => CreateTestMessage($"msg-{i}", "arn:aws:sns:us-east-1:123456789012:topic2")).ToList();
        
        foreach (var message in topic1Messages.Concat(topic2Messages))
        {
            _store.Add(message);
        }
        
        // Act
        var result = _store.GetMessagesByTopic("arn:aws:sns:us-east-1:123456789012:topic1");
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data!.Count).IsEqualTo(2);
        await Assert.That(result.Data!.All(m => m.TopicArn == "arn:aws:sns:us-east-1:123456789012:topic1")).IsTrue();
    }
    
    [Test]
    public async Task GetMessagesByTopic_Should_Return_Failure_For_Null_TopicArn()
    {
        // Act
        var result = _store.GetMessagesByTopic(null!);
        
        // Assert
        await Assert.That(!result.Success).IsTrue();
        await Assert.That(result.Exception).IsTypeOf<MessageStoreError>();
        var error = (MessageStoreError)result.Exception!;
        await Assert.That(error.Code).IsEqualTo("NULL_TOPIC_ARN");
    }
    
    [Test]
    public async Task GetMessagesByTimeRange_Should_Filter_Correctly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var messages = new[]
        {
            CreateTestMessage("msg-1", timestamp: now.AddMinutes(-5)),
            CreateTestMessage("msg-2", timestamp: now.AddMinutes(-3)),
            CreateTestMessage("msg-3", timestamp: now.AddMinutes(-1)),
            CreateTestMessage("msg-4", timestamp: now.AddMinutes(1))
        };
        
        foreach (var message in messages)
        {
            _store.Add(message);
        }
        
        // Act
        var result = _store.GetMessagesByTimeRange(now.AddMinutes(-4), now);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data!.Count).IsEqualTo(2);
        await Assert.That(result.Data!.Select(m => m.Id))
            .IsEquivalentTo(new[] { "msg-2", "msg-3" });
    }
    
    [Test]
    public async Task GetMessageById_Should_Return_Correct_Message()
    {
        // Arrange
        var messages = Enumerable.Range(1, 3).Select(i => CreateTestMessage($"msg-{i}")).ToList();
        foreach (var message in messages)
        {
            _store.Add(message);
        }
        
        // Act
        var result = _store.GetMessageById("msg-2");
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data!.Id).IsEqualTo("msg-2");
    }
    
    [Test]
    public async Task GetMessageById_Should_Return_Failure_For_NotFound()
    {
        // Act
        var result = _store.GetMessageById("non-existent");
        
        // Assert
        await Assert.That(!result.Success).IsTrue();
        await Assert.That(result.Exception).IsTypeOf<MessageStoreError>();
        var error = (MessageStoreError)result.Exception!;
        await Assert.That(error.Code).IsEqualTo("MESSAGE_NOT_FOUND");
    }
    
    [Test]
    public async Task GetMessageById_Should_Return_Failure_For_Null_Id()
    {
        // Act
        var result = _store.GetMessageById(null!);
        
        // Assert
        await Assert.That(!result.Success).IsTrue();
        await Assert.That(result.Exception).IsTypeOf<MessageStoreError>();
        var error = (MessageStoreError)result.Exception!;
        await Assert.That(error.Code).IsEqualTo("NULL_MESSAGE_ID");
    }
    
    [Test]
    public async Task Clear_Should_Remove_All_Messages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 3).Select(i => CreateTestMessage($"msg-{i}")).ToList();
        foreach (var message in messages)
        {
            _store.Add(message);
        }
        
        // Act
        var result = _store.Clear();
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(_store.Count).IsEqualTo(0);
        var getResult = _store.GetMessages();
        await Assert.That(getResult.Data!.Count).IsEqualTo(0);
    }
    
    [Test]
    [Repeat(10)]
    public async Task Concurrent_Operations_Should_Be_ThreadSafe()
    {
        // Arrange
        var store = new InMemoryMessageStore(maxMessages: 1000);
        var messageCount = 50;
        var tasks = new List<Task>();
        
        // Act - Concurrent adds
        for (int i = 0; i < messageCount; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                var message = CreateTestMessage($"concurrent-{index}");
                var result = store.Add(message);
                await Assert.That(result.Success).IsTrue();
            }));
        }
        
        // Concurrent reads
        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = store.GetMessages();
                await Assert.That(result.Success).IsTrue();
            }));
        }
        
        // Concurrent queries
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = store.GetMessagesByTopic("arn:aws:sns:us-east-1:123456789012:test-topic");
                await Assert.That(result.Success).IsTrue();
            }));
        }
        
        await Task.WhenAll(tasks);
        
        // Assert
        await Assert.That(store.Count).IsEqualTo(messageCount);
    }
    
    [Test]
    public async Task Query_Performance_Should_Be_Fast_For_Large_Dataset()
    {
        // Arrange
        var store = new InMemoryMessageStore(maxMessages: 10000);
        for (int i = 0; i < 10000; i++)
        {
            store.Add(CreateTestMessage($"msg-{i}", topicArn: $"topic-{i % 10}"));
        }
        
        var stopwatch = new Stopwatch();
        
        // Act - Query all messages
        stopwatch.Start();
        var result = store.GetMessages();
        stopwatch.Stop();
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data!.Count).IsEqualTo(10000);
        await Assert.That(stopwatch.Elapsed.TotalMilliseconds).IsLessThan(1); // <1ms for 10k messages
    }
    
    private static CapturedSnsMessage CreateTestMessage(
        string id, 
        string? topicArn = null,
        DateTimeOffset? timestamp = null)
    {
        return new CapturedSnsMessage
        {
            Id = id,
            TopicArn = topicArn ?? "arn:aws:sns:us-east-1:123456789012:test-topic",
            Message = $"Test message content for {id}",
            Subject = $"Test Subject {id}",
            Timestamp = timestamp ?? DateTimeOffset.UtcNow,
            RawPayload = $"{{\"id\":\"{id}\"}}"
        };
    }
}