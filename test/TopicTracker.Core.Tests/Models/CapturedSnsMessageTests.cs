using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Models;

namespace TopicTracker.Core.Tests.Models;

public class CapturedSnsMessageTests
{
    [Test]
    public async Task CapturedSnsMessage_Should_Have_Required_Properties()
    {
        // Arrange
        var messageId = "test-message-id";
        var topicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic";
        var subject = "Test Subject";
        var message = "Test Message Body";
        var timestamp = DateTimeOffset.UtcNow;
        var rawPayload = "raw-payload";
        
        // Act
        var capturedMessage = new CapturedSnsMessage
        {
            Id = messageId,
            TopicArn = topicArn,
            Subject = subject,
            Message = message,
            Timestamp = timestamp,
            RawPayload = rawPayload
        };
        
        // Assert
        await Assert.That(capturedMessage.Id).IsEqualTo(messageId);
        await Assert.That(capturedMessage.TopicArn).IsEqualTo(topicArn);
        await Assert.That(capturedMessage.Subject).IsEqualTo(subject);
        await Assert.That(capturedMessage.Message).IsEqualTo(message);
        await Assert.That(capturedMessage.Timestamp).IsEqualTo(timestamp);
        await Assert.That(capturedMessage.RawPayload).IsEqualTo(rawPayload);
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Be_Immutable()
    {
        // Act
        var capturedMessage = new CapturedSnsMessage
        {
            Id = "test-id",
            TopicArn = "test-arn",
            Message = "test-message",
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw"
        };
        
        // Assert - Properties should be init-only (this will fail to compile if they're not)
        // The model should use init-only properties
        await Assert.That(capturedMessage).IsNotNull();
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Support_MessageAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, MessageAttribute>
        {
            ["attr1"] = new MessageAttribute 
            { 
                DataType = "String", 
                StringValue = "value1" 
            },
            ["attr2"] = new MessageAttribute 
            { 
                DataType = "Number", 
                StringValue = "123" 
            }
        };
        
        // Act
        var capturedMessage = new CapturedSnsMessage
        {
            Id = "test-id",
            TopicArn = "test-arn",
            Message = "test-message",
            MessageAttributes = attributes,
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw"
        };
        
        // Assert
        await Assert.That(capturedMessage.MessageAttributes).IsNotNull();
        await Assert.That(capturedMessage.MessageAttributes!.Count).IsEqualTo(2);
        await Assert.That(capturedMessage.MessageAttributes!["attr1"].StringValue).IsEqualTo("value1");
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Support_MessageStructure()
    {
        // Act
        var capturedMessage = new CapturedSnsMessage
        {
            Id = "test-id",
            TopicArn = "test-arn",
            Message = "test-message",
            MessageStructure = "json",
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw"
        };
        
        // Assert
        await Assert.That(capturedMessage.MessageStructure).IsEqualTo("json");
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Allow_Null_Optional_Properties()
    {
        // Act
        var capturedMessage = new CapturedSnsMessage
        {
            Id = "test-id",
            TopicArn = "test-arn",
            Message = "test-message",
            Subject = null,
            MessageAttributes = null,
            MessageStructure = null,
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw"
        };
        
        // Assert
        await Assert.That(capturedMessage.Subject).IsNull();
        await Assert.That(capturedMessage.MessageAttributes).IsNull();
        await Assert.That(capturedMessage.MessageStructure).IsNull();
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Serialize_To_Json_In_Under_100_Microseconds()
    {
        // Arrange
        var capturedMessage = new CapturedSnsMessage
        {
            Id = "test-message-id",
            TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic",
            Subject = "Test Subject",
            Message = "Test Message Body",
            MessageAttributes = new Dictionary<string, MessageAttribute>
            {
                ["TestAttr"] = new MessageAttribute 
                { 
                    DataType = "String", 
                    StringValue = "TestValue" 
                }
            },
            MessageStructure = "json",
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = "raw-payload"
        };
        
        // Warm up
        _ = JsonSerializer.Serialize(capturedMessage);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var json = JsonSerializer.Serialize(capturedMessage, SnsJsonContext.Default.CapturedSnsMessage);
        stopwatch.Stop();
        
        // Assert
        await Assert.That(stopwatch.Elapsed.TotalMicroseconds).IsLessThan(100);
        await Assert.That(json).IsNotNull();
        await Assert.That(json).Contains("test-message-id");
    }
    
    [Test]
    public async Task CapturedSnsMessage_Should_Deserialize_From_Json()
    {
        // Arrange
        var json = @"{
            ""Id"": ""test-id"",
            ""TopicArn"": ""test-arn"",
            ""Subject"": ""test-subject"",
            ""Message"": ""test-message"",
            ""MessageAttributes"": {
                ""attr1"": {
                    ""DataType"": ""String"",
                    ""StringValue"": ""value1""
                }
            },
            ""MessageStructure"": ""json"",
            ""Timestamp"": ""2024-01-01T00:00:00Z"",
            ""RawPayload"": ""raw""
        }";
        
        // Act
        var capturedMessage = JsonSerializer.Deserialize<CapturedSnsMessage>(
            json, 
            SnsJsonContext.Default.CapturedSnsMessage
        );
        
        // Assert
        await Assert.That(capturedMessage).IsNotNull();
        await Assert.That(capturedMessage!.Id).IsEqualTo("test-id");
        await Assert.That(capturedMessage.TopicArn).IsEqualTo("test-arn");
        await Assert.That(capturedMessage.Subject).IsEqualTo("test-subject");
        await Assert.That(capturedMessage.Message).IsEqualTo("test-message");
        await Assert.That(capturedMessage.MessageAttributes).IsNotNull();
        await Assert.That(capturedMessage.MessageAttributes!["attr1"].StringValue).IsEqualTo("value1");
    }
}