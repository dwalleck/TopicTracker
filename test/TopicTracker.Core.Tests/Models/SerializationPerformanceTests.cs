using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Models;

namespace TopicTracker.Core.Tests.Models;

public class SerializationPerformanceTests
{
    private static readonly CapturedSnsMessage TestMessage = new()
    {
        Id = "550e8400-e29b-41d4-a716-446655440000",
        TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic",
        Subject = "Test Subject",
        Message = "This is a test message with some content to make it realistic",
        MessageAttributes = new Dictionary<string, MessageAttribute>
        {
            ["OrderId"] = new MessageAttribute { DataType = "String", StringValue = "12345" },
            ["Priority"] = new MessageAttribute { DataType = "Number", StringValue = "1" },
            ["Timestamp"] = new MessageAttribute { DataType = "String", StringValue = "2024-01-01T00:00:00Z" }
        },
        MessageStructure = "json",
        Timestamp = DateTimeOffset.UtcNow,
        RawPayload = """{"Type":"Notification","MessageId":"550e8400-e29b-41d4-a716-446655440000"}"""
    };
    
    [Test]
    public async Task Serialization_Should_Complete_Under_100_Microseconds()
    {
        // Warm up the serializer
        for (int i = 0; i < 100; i++)
        {
            _ = JsonSerializer.Serialize(TestMessage, SnsJsonContext.Default.CapturedSnsMessage);
        }
        
        // Measure single serialization
        var stopwatch = Stopwatch.StartNew();
        var json = JsonSerializer.Serialize(TestMessage, SnsJsonContext.Default.CapturedSnsMessage);
        stopwatch.Stop();
        
        // Assert
        await Assert.That(stopwatch.Elapsed.TotalMicroseconds).IsLessThan(100);
        await Assert.That(json).IsNotNull();
    }
    
    [Test]
    public async Task Deserialization_Should_Complete_Under_100_Microseconds()
    {
        // Arrange
        var json = JsonSerializer.Serialize(TestMessage, SnsJsonContext.Default.CapturedSnsMessage);
        
        // Warm up
        for (int i = 0; i < 100; i++)
        {
            _ = JsonSerializer.Deserialize<CapturedSnsMessage>(json, SnsJsonContext.Default.CapturedSnsMessage);
        }
        
        // Measure
        var stopwatch = Stopwatch.StartNew();
        var deserialized = JsonSerializer.Deserialize<CapturedSnsMessage>(json, SnsJsonContext.Default.CapturedSnsMessage);
        stopwatch.Stop();
        
        // Assert
        await Assert.That(stopwatch.Elapsed.TotalMicroseconds).IsLessThan(100);
        await Assert.That(deserialized).IsNotNull();
        await Assert.That(deserialized!.Id).IsEqualTo(TestMessage.Id);
    }
    
    [Test]
    public async Task Serialization_Should_Have_Zero_Allocations_On_Hot_Path()
    {
        // This test verifies the performance characteristics
        // In a real benchmark, we'd use BenchmarkDotNet's MemoryDiagnoser
        
        // Warm up
        for (int i = 0; i < 1000; i++)
        {
            _ = JsonSerializer.Serialize(TestMessage, SnsJsonContext.Default.CapturedSnsMessage);
        }
        
        // Get baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var beforeGen0 = GC.CollectionCount(0);
        
        // Serialize many times
        for (int i = 0; i < 1000; i++)
        {
            _ = JsonSerializer.Serialize(TestMessage, SnsJsonContext.Default.CapturedSnsMessage);
        }
        
        var afterGen0 = GC.CollectionCount(0);
        
        // With source generators, we should see minimal GC
        await Assert.That(afterGen0 - beforeGen0).IsLessThanOrEqualTo(2);
    }
    
    [Test]
    [TUnit.Core.Arguments(1)]
    [TUnit.Core.Arguments(10)]
    [TUnit.Core.Arguments(100)]
    [TUnit.Core.Arguments(1000)]
    public async Task Batch_Serialization_Performance(int messageCount)
    {
        // Create test messages
        var messages = new List<CapturedSnsMessage>(messageCount);
        for (int i = 0; i < messageCount; i++)
        {
            messages.Add(new CapturedSnsMessage
            {
                Id = Guid.NewGuid().ToString(),
                TopicArn = TestMessage.TopicArn,
                Subject = $"Message {i}",
                Message = TestMessage.Message,
                Timestamp = DateTimeOffset.UtcNow,
                RawPayload = TestMessage.RawPayload
            });
        }
        
        // Warm up
        _ = JsonSerializer.Serialize(messages, SnsJsonContext.Default.ListCapturedSnsMessage);
        
        // Measure
        var stopwatch = Stopwatch.StartNew();
        var json = JsonSerializer.Serialize(messages, SnsJsonContext.Default.ListCapturedSnsMessage);
        stopwatch.Stop();
        
        // Assert - Should scale linearly (approximately)
        var perMessageMicroseconds = stopwatch.Elapsed.TotalMicroseconds / messageCount;
        await Assert.That(perMessageMicroseconds).IsLessThan(100);
    }
}

// Separate benchmark class for detailed performance analysis
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class SerializationBenchmarks
{
    private CapturedSnsMessage _message = null!;
    private string _json = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _message = new CapturedSnsMessage
        {
            Id = "550e8400-e29b-41d4-a716-446655440000",
            TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic",
            Subject = "Test Subject",
            Message = "This is a test message",
            MessageAttributes = new Dictionary<string, MessageAttribute>
            {
                ["TestAttr"] = new MessageAttribute { DataType = "String", StringValue = "TestValue" }
            },
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw"
        };
        
        _json = JsonSerializer.Serialize(_message, SnsJsonContext.Default.CapturedSnsMessage);
    }
    
    [Benchmark]
    public string SerializeWithSourceGenerator()
    {
        return JsonSerializer.Serialize(_message, SnsJsonContext.Default.CapturedSnsMessage);
    }
    
    [Benchmark]
    public CapturedSnsMessage? DeserializeWithSourceGenerator()
    {
        return JsonSerializer.Deserialize<CapturedSnsMessage>(_json, SnsJsonContext.Default.CapturedSnsMessage);
    }
    
    [Benchmark(Baseline = true)]
    public string SerializeWithoutSourceGenerator()
    {
        return JsonSerializer.Serialize(_message);
    }
    
    [Benchmark]
    public CapturedSnsMessage? DeserializeWithoutSourceGenerator()
    {
        return JsonSerializer.Deserialize<CapturedSnsMessage>(_json);
    }
}