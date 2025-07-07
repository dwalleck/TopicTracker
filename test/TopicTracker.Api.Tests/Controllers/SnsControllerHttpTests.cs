using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions;
using TUnit.Core;
using TopicTracker.Core.Storage;

namespace TopicTracker.Api.Tests.Controllers;

/// <summary>
/// Direct HTTP integration tests for SnsController that bypass AWS SDK compatibility issues
/// </summary>
public class SnsControllerHttpTests : IAsyncDisposable
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private IMessageStore _messageStore = null!;

    [Before(Test)]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
        
        // Get the message store to verify captured messages
        using var scope = _factory.Services.CreateScope();
        _messageStore = scope.ServiceProvider.GetRequiredService<IMessageStore>();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task SnsEndpoint_Should_Accept_Publish_Request()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", "Test message")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var messageId = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "MessageId")?.Value;
        
        await Assert.That(messageId).IsNotNull();
        await Assert.That(messageId).IsNotEmpty();
        
        // Verify message was captured
        var messages = _messageStore.GetMessages();
        await Assert.That(messages.Success).IsTrue();
        await Assert.That(messages.Data!.Count).IsEqualTo(1);
        await Assert.That(messages.Data![0].Message).IsEqualTo("Test message");
        await Assert.That(messages.Data![0].TopicArn).IsEqualTo("arn:aws:sns:us-east-1:123456789012:test-topic");
    }

    [Test]
    public async Task SnsEndpoint_Should_Accept_Publish_Request_With_Subject()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", "Test message body"),
            new KeyValuePair<string, string>("Subject", "Test Subject")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Verify captured message includes subject
        var messages = _messageStore.GetMessages();
        await Assert.That(messages.Data![0].Subject).IsEqualTo("Test Subject");
    }

    [Test]
    public async Task SnsEndpoint_Should_Accept_Publish_Request_With_MessageAttributes()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", "Test message with attributes"),
            new KeyValuePair<string, string>("MessageAttributes.entry.1.Name", "TestKey"),
            new KeyValuePair<string, string>("MessageAttributes.entry.1.Value.DataType", "String"),
            new KeyValuePair<string, string>("MessageAttributes.entry.1.Value.StringValue", "TestValue"),
            new KeyValuePair<string, string>("MessageAttributes.entry.2.Name", "NumberKey"),
            new KeyValuePair<string, string>("MessageAttributes.entry.2.Value.DataType", "Number"),
            new KeyValuePair<string, string>("MessageAttributes.entry.2.Value.StringValue", "123")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Verify message attributes were captured
        var messages = _messageStore.GetMessages();
        var capturedMessage = messages.Data![0];
        await Assert.That(capturedMessage.MessageAttributes).IsNotNull();
        await Assert.That(capturedMessage.MessageAttributes!.Count).IsEqualTo(2);
        await Assert.That(capturedMessage.MessageAttributes!["TestKey"].DataType).IsEqualTo("String");
        await Assert.That(capturedMessage.MessageAttributes!["TestKey"].StringValue).IsEqualTo("TestValue");
        await Assert.That(capturedMessage.MessageAttributes!["NumberKey"].DataType).IsEqualTo("Number");
        await Assert.That(capturedMessage.MessageAttributes!["NumberKey"].StringValue).IsEqualTo("123");
    }

    [Test]
    public async Task SnsEndpoint_Should_Handle_Multiple_Concurrent_Requests()
    {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();
        
        for (int i = 0; i < 10; i++)
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Action", "Publish"),
                new KeyValuePair<string, string>("TopicArn", $"arn:aws:sns:us-east-1:123456789012:topic-{i}"),
                new KeyValuePair<string, string>("Message", $"Concurrent message {i}")
            });
            tasks.Add(_client.PostAsync("/", formData));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }
        
        var messages = _messageStore.GetMessages();
        await Assert.That(messages.Data!.Count).IsEqualTo(10);
    }

    [Test]
    public async Task SnsEndpoint_Should_Handle_CreateTopic_Request()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "CreateTopic"),
            new KeyValuePair<string, string>("Name", "test-topic-creation")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var topicArn = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "TopicArn")?.Value;
        
        await Assert.That(topicArn).IsNotNull();
        await Assert.That(topicArn).Contains("test-topic-creation");
    }

    [Test]
    public async Task SnsEndpoint_Should_Return_Error_For_Missing_TopicArn()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("Message", "Message without topic")
            // TopicArn is missing
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var errorCode = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
        var errorMessage = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Message")?.Value;
        
        await Assert.That(errorCode).IsEqualTo("InvalidParameter");
        await Assert.That(errorMessage).Contains("TopicArn");
    }

    [Test]
    public async Task SnsEndpoint_Should_Return_Error_For_Missing_Message()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic")
            // Message is missing
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var errorCode = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
        var errorMessage = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Message")?.Value;
        
        await Assert.That(errorCode).IsEqualTo("InvalidParameter");
        await Assert.That(errorMessage).Contains("Message");
    }

    [Test]
    public async Task SnsEndpoint_Should_Handle_JSON_MessageStructure()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", @"{
                ""default"": ""Default message"",
                ""email"": ""Email specific message"",
                ""sms"": ""SMS specific message""
            }"),
            new KeyValuePair<string, string>("MessageStructure", "json")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        var messages = _messageStore.GetMessages();
        var capturedMessage = messages.Data![0];
        await Assert.That(capturedMessage.MessageStructure).IsEqualTo("json");
    }

    [Test]
    public async Task SnsEndpoint_Should_Handle_MessageDeduplicationId()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic.fifo"),
            new KeyValuePair<string, string>("Message", "Test message with deduplication"),
            new KeyValuePair<string, string>("MessageDeduplicationId", "unique-dedup-id-123")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Send duplicate
        var duplicateResponse = await _client.PostAsync("/", formData);
        await Assert.That(duplicateResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
        
        // Should only have one message due to deduplication
        var messages = _messageStore.GetMessages();
        await Assert.That(messages.Data!.Count).IsEqualTo(1);
        await Assert.That(messages.Data![0].MessageDeduplicationId).IsEqualTo("unique-dedup-id-123");
    }

    [Test]
    public async Task SnsEndpoint_Response_Time_Should_Be_Under_1ms()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "Publish"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", "Performance test message")
        });
        
        // Warm up
        await _client.PostAsync("/", formData);
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.PostAsync("/", formData);
        stopwatch.Stop();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThanOrEqualTo(1);
    }

    [Test]
    public async Task SnsEndpoint_Should_Return_Error_For_Unknown_Action()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Action", "UnknownAction"),
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic")
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var errorCode = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
        
        await Assert.That(errorCode).IsEqualTo("InvalidAction");
    }

    [Test]
    public async Task SnsEndpoint_Should_Return_Error_For_Missing_Action()
    {
        // Arrange
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
            new KeyValuePair<string, string>("Message", "Test message")
            // Action is missing
        });

        // Act
        var response = await _client.PostAsync("/", formData);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        
        var content = await response.Content.ReadAsStringAsync();
        var xml = XDocument.Parse(content);
        var errorCode = xml.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "Code")?.Value;
        
        await Assert.That(errorCode).IsEqualTo("MissingAction");
    }
}