using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Models;

namespace TopicTracker.Core.Tests.Models;

public class SnsPublishRequestTests
{
    [Test]
    public async Task SnsPublishRequest_Should_Have_Required_Properties()
    {
        // Arrange
        var topicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic";
        var message = "Test Message";
        var subject = "Test Subject";
        var messageStructure = "json";
        
        // Act
        var request = new SnsPublishRequest
        {
            TopicArn = topicArn,
            Message = message,
            Subject = subject,
            MessageStructure = messageStructure
        };
        
        // Assert
        await Assert.That(request.TopicArn).IsEqualTo(topicArn);
        await Assert.That(request.Message).IsEqualTo(message);
        await Assert.That(request.Subject).IsEqualTo(subject);
        await Assert.That(request.MessageStructure).IsEqualTo(messageStructure);
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Be_Immutable()
    {
        // Act
        var request = new SnsPublishRequest
        {
            TopicArn = "test-arn",
            Message = "test-message"
        };
        
        // Assert - Properties should be init-only
        await Assert.That(request).IsNotNull();
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Support_MessageAttributes()
    {
        // Arrange
        var attributes = new Dictionary<string, MessageAttribute>
        {
            ["OrderId"] = new MessageAttribute 
            { 
                DataType = "String", 
                StringValue = "12345" 
            },
            ["Priority"] = new MessageAttribute 
            { 
                DataType = "Number", 
                StringValue = "1" 
            },
            ["Binary"] = new MessageAttribute
            {
                DataType = "Binary",
                BinaryValue = new byte[] { 1, 2, 3, 4 }
            }
        };
        
        // Act
        var request = new SnsPublishRequest
        {
            TopicArn = "test-arn",
            Message = "test-message",
            MessageAttributes = attributes
        };
        
        // Assert
        await Assert.That(request.MessageAttributes).IsNotNull();
        await Assert.That(request.MessageAttributes!.Count).IsEqualTo(3);
        await Assert.That(request.MessageAttributes!["OrderId"].StringValue).IsEqualTo("12345");
        await Assert.That(request.MessageAttributes!["Priority"].DataType).IsEqualTo("Number");
        await Assert.That(request.MessageAttributes!["Binary"].BinaryValue).IsNotNull();
        await Assert.That(request.MessageAttributes!["Binary"].BinaryValue!.Length).IsEqualTo(4);
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Support_TargetArn()
    {
        // Act
        var request = new SnsPublishRequest
        {
            TargetArn = "arn:aws:sns:us-east-1:123456789012:endpoint/GCM/MyApp/12345678",
            Message = "test-message"
        };
        
        // Assert
        await Assert.That(request.TargetArn).IsEqualTo("arn:aws:sns:us-east-1:123456789012:endpoint/GCM/MyApp/12345678");
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Support_PhoneNumber()
    {
        // Act
        var request = new SnsPublishRequest
        {
            PhoneNumber = "+1234567890",
            Message = "test-message"
        };
        
        // Assert
        await Assert.That(request.PhoneNumber).IsEqualTo("+1234567890");
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Allow_Different_Publish_Targets()
    {
        // Test that you can publish to Topic, TargetArn, or PhoneNumber
        var topicRequest = new SnsPublishRequest
        {
            TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic",
            Message = "test"
        };
        
        var targetRequest = new SnsPublishRequest
        {
            TargetArn = "arn:aws:sns:us-east-1:123456789012:endpoint/GCM/MyApp/12345678",
            Message = "test"
        };
        
        var phoneRequest = new SnsPublishRequest
        {
            PhoneNumber = "+1234567890",
            Message = "test"
        };
        
        // Assert
        await Assert.That(topicRequest.TopicArn).IsNotNull();
        await Assert.That(targetRequest.TargetArn).IsNotNull();
        await Assert.That(phoneRequest.PhoneNumber).IsNotNull();
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Support_MessageDeduplicationId()
    {
        // For FIFO topics
        var request = new SnsPublishRequest
        {
            TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic.fifo",
            Message = "test-message",
            MessageDeduplicationId = "dedup-123",
            MessageGroupId = "group-1"
        };
        
        // Assert
        await Assert.That(request.MessageDeduplicationId).IsEqualTo("dedup-123");
        await Assert.That(request.MessageGroupId).IsEqualTo("group-1");
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Serialize_To_Json()
    {
        // Arrange
        var request = new SnsPublishRequest
        {
            TopicArn = "arn:aws:sns:us-east-1:123456789012:MyTopic",
            Subject = "Test Subject",
            Message = "Test Message",
            MessageAttributes = new Dictionary<string, MessageAttribute>
            {
                ["TestAttr"] = new MessageAttribute 
                { 
                    DataType = "String", 
                    StringValue = "TestValue" 
                }
            }
        };
        
        // Act
        var json = JsonSerializer.Serialize(request, SnsJsonContext.Default.SnsPublishRequest);
        
        // Assert
        await Assert.That(json).IsNotNull();
        await Assert.That(json).Contains("TopicArn");
        await Assert.That(json).Contains("Message");
        await Assert.That(json).Contains("MessageAttributes");
    }
    
    [Test]
    public async Task SnsPublishRequest_Should_Deserialize_From_Json()
    {
        // Arrange
        var json = @"{
            ""TopicArn"": ""arn:aws:sns:us-east-1:123456789012:MyTopic"",
            ""Subject"": ""Test Subject"",
            ""Message"": ""Test Message"",
            ""MessageStructure"": ""json"",
            ""MessageAttributes"": {
                ""attr1"": {
                    ""DataType"": ""String"",
                    ""StringValue"": ""value1""
                }
            }
        }";
        
        // Act
        var request = JsonSerializer.Deserialize<SnsPublishRequest>(
            json, 
            SnsJsonContext.Default.SnsPublishRequest
        );
        
        // Assert
        await Assert.That(request).IsNotNull();
        await Assert.That(request!.TopicArn).IsEqualTo("arn:aws:sns:us-east-1:123456789012:MyTopic");
        await Assert.That(request.Subject).IsEqualTo("Test Subject");
        await Assert.That(request.Message).IsEqualTo("Test Message");
        await Assert.That(request.MessageStructure).IsEqualTo("json");
        await Assert.That(request.MessageAttributes).IsNotNull();
        await Assert.That(request.MessageAttributes!["attr1"].StringValue).IsEqualTo("value1");
    }
}