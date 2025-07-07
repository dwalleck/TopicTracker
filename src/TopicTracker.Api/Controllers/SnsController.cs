using System.Collections.Specialized;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tethys.Results;
using TopicTracker.Core.Models;
using TopicTracker.Core.Storage;

namespace TopicTracker.Api.Controllers;

[ApiController]
[Route("/")]
public class SnsController : ControllerBase
{
    private readonly IMessageStore _messageStore;
    private readonly ILogger<SnsController> _logger;

    public SnsController(IMessageStore messageStore, ILogger<SnsController> logger)
    {
        _messageStore = messageStore ?? throw new ArgumentNullException(nameof(messageStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    public async Task<IActionResult> HandleSnsRequest()
    {
        // Read the request body
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        // AWS SDK sends form-encoded data
        var formData = System.Web.HttpUtility.ParseQueryString(body);
        var action = formData["Action"];

        // Also check X-Amz-Target header for newer SDK versions
        if (string.IsNullOrEmpty(action))
        {
            var target = Request.Headers["X-Amz-Target"].FirstOrDefault();
            if (!string.IsNullOrEmpty(target))
            {
                action = target.Split('.').LastOrDefault();
            }
        }

        if (string.IsNullOrEmpty(action))
        {
            return CreateAwsErrorResponse(400, "MissingAction", 
                "Could not find operation to perform. Action should be passed as form parameter or in X-Amz-Target header.");
        }

        return action switch
        {
            "Publish" => await HandlePublishFormData(formData, body),
            "CreateTopic" => await HandleCreateTopicFormData(formData),
            _ => CreateAwsErrorResponse(400, "InvalidAction", 
                $"The action {action} is not valid for this web service.")
        };
    }

    private Task<IActionResult> HandlePublishFormData(NameValueCollection formData, string rawBody)
    {
        var topicArn = formData["TopicArn"];
        var message = formData["Message"];
        var subject = formData["Subject"];
        var messageStructure = formData["MessageStructure"];
        var messageDeduplicationId = formData["MessageDeduplicationId"];
        var messageGroupId = formData["MessageGroupId"];

        // Validate required fields
        if (string.IsNullOrEmpty(topicArn))
        {
            return Task.FromResult<IActionResult>(CreateAwsErrorResponse(400, "InvalidParameter", 
                "Invalid parameter: TopicArn Reason: Must be specified."));
        }

        if (string.IsNullOrEmpty(message))
        {
            return Task.FromResult<IActionResult>(CreateAwsErrorResponse(400, "InvalidParameter", 
                "Invalid parameter: Message Reason: Must be specified."));
        }

        // Handle deduplication for FIFO topics
        if (!string.IsNullOrEmpty(messageDeduplicationId))
        {
            var existingMessages = _messageStore.GetMessages();
            if (existingMessages.Success && existingMessages.Data != null)
            {
                var duplicate = existingMessages.Data.FirstOrDefault(m => 
                    m.MessageDeduplicationId == messageDeduplicationId && 
                    m.TopicArn == topicArn);
                
                if (duplicate != null)
                {
                    // Return the same message ID for duplicates
                    return Task.FromResult<IActionResult>(CreatePublishSuccessResponse(duplicate.Id));
                }
            }
        }

        // Parse message attributes
        var messageAttributes = ParseMessageAttributes(formData);

        // Create captured message
        var messageId = Guid.NewGuid().ToString();
        var capturedMessage = new CapturedSnsMessage
        {
            Id = messageId,
            TopicArn = topicArn,
            Message = message,
            Subject = subject,
            MessageStructure = messageStructure,
            MessageDeduplicationId = messageDeduplicationId,
            MessageGroupId = messageGroupId,
            MessageAttributes = messageAttributes,
            Timestamp = DateTimeOffset.UtcNow,
            RawPayload = rawBody
        };

        // Store the message
        var result = _messageStore.Add(capturedMessage);
        if (!result.Success)
        {
            _logger.LogError("Failed to store message: {Error}", result.Exception?.Message);
            return Task.FromResult<IActionResult>(CreateAwsErrorResponse(500, "InternalError", 
                "An error occurred while processing the request"));
        }

        return Task.FromResult<IActionResult>(CreatePublishSuccessResponse(messageId));
    }

    private Task<IActionResult> HandleCreateTopicFormData(NameValueCollection formData)
    {
        var name = formData["Name"];
        
        if (string.IsNullOrEmpty(name))
        {
            return Task.FromResult<IActionResult>(CreateAwsErrorResponse(400, "InvalidParameter", 
                "Invalid parameter: Name Reason: Must be specified."));
        }

        // Generate a topic ARN
        var topicArn = $"arn:aws:sns:us-east-1:123456789012:{name}";

        return Task.FromResult<IActionResult>(CreateTopicSuccessResponse(topicArn));
    }

    private Dictionary<string, MessageAttribute>? ParseMessageAttributes(NameValueCollection formData)
    {
        var attributes = new Dictionary<string, MessageAttribute>();
        int index = 1;
        
        while (true)
        {
            var nameKey = $"MessageAttributes.entry.{index}.Name";
            var name = formData[nameKey];
            
            if (string.IsNullOrEmpty(name))
                break;
                
            var dataType = formData[$"MessageAttributes.entry.{index}.Value.DataType"] ?? "String";
            var stringValue = formData[$"MessageAttributes.entry.{index}.Value.StringValue"];
            var binaryValue = formData[$"MessageAttributes.entry.{index}.Value.BinaryValue"];
            
            attributes[name] = new MessageAttribute
            {
                DataType = dataType,
                StringValue = stringValue,
                BinaryValue = !string.IsNullOrEmpty(binaryValue) ? Convert.FromBase64String(binaryValue) : null
            };
            
            index++;
        }
        
        return attributes.Count > 0 ? attributes : null;
    }

    private IActionResult CreatePublishSuccessResponse(string messageId)
    {
        var xmlResponse = $@"<PublishResponse xmlns=""http://sns.amazonaws.com/doc/2010-03-31/"">
    <PublishResult>
        <MessageId>{messageId}</MessageId>
    </PublishResult>
    <ResponseMetadata>
        <RequestId>{Guid.NewGuid()}</RequestId>
    </ResponseMetadata>
</PublishResponse>";

        return new ContentResult
        {
            Content = xmlResponse,
            ContentType = "text/xml",
            StatusCode = 200
        };
    }

    private IActionResult CreateTopicSuccessResponse(string topicArn)
    {
        var xmlResponse = $@"<CreateTopicResponse xmlns=""http://sns.amazonaws.com/doc/2010-03-31/"">
    <CreateTopicResult>
        <TopicArn>{topicArn}</TopicArn>
    </CreateTopicResult>
    <ResponseMetadata>
        <RequestId>{Guid.NewGuid()}</RequestId>
    </ResponseMetadata>
</CreateTopicResponse>";

        return new ContentResult
        {
            Content = xmlResponse,
            ContentType = "text/xml",
            StatusCode = 200
        };
    }

    private IActionResult CreateAwsErrorResponse(int statusCode, string code, string message)
    {
        var xmlResponse = $@"<ErrorResponse xmlns=""http://sns.amazonaws.com/doc/2010-03-31/"">
    <Error>
        <Type>Sender</Type>
        <Code>{code}</Code>
        <Message>{message}</Message>
    </Error>
    <RequestId>{Guid.NewGuid()}</RequestId>
</ErrorResponse>";

        return new ContentResult
        {
            Content = xmlResponse,
            ContentType = "text/xml",
            StatusCode = statusCode
        };
    }

    // The JSON handling methods below are kept for potential future use with newer SDK versions
    private Task<IActionResult> HandlePublish(string body)
    {
        try
        {
            var request = JsonSerializer.Deserialize<PublishRequest>(body);
            if (request == null)
            {
                return Task.FromResult<IActionResult>(BadRequest(new
                {
                    __type = "InvalidParameterValue",
                    message = "Invalid request body"
                }));
            }

            // Validate required fields
            if (string.IsNullOrEmpty(request.TopicArn))
            {
                return Task.FromResult<IActionResult>(BadRequest(new
                {
                    __type = "InvalidParameter",
                    message = "Invalid parameter: TopicArn Reason: Must be specified."
                }));
            }

            if (string.IsNullOrEmpty(request.Message))
            {
                return Task.FromResult<IActionResult>(BadRequest(new
                {
                    __type = "InvalidParameter",
                    message = "Invalid parameter: Message Reason: Must be specified."
                }));
            }

            // Handle deduplication for FIFO topics
            if (!string.IsNullOrEmpty(request.MessageDeduplicationId))
            {
                var existingMessages = _messageStore.GetMessages();
                if (existingMessages.Success && existingMessages.Data != null)
                {
                    var duplicate = existingMessages.Data.FirstOrDefault(m => 
                        m.MessageDeduplicationId == request.MessageDeduplicationId && 
                        m.TopicArn == request.TopicArn);
                    
                    if (duplicate != null)
                    {
                        // Return the same message ID for duplicates
                        return Task.FromResult<IActionResult>(Ok(new PublishResponse { MessageId = duplicate.Id }));
                    }
                }
            }

            // Create captured message
            var messageId = Guid.NewGuid().ToString();
            
            // Convert message attributes if present
            Dictionary<string, MessageAttribute>? messageAttributes = null;
            if (request.MessageAttributes != null)
            {
                messageAttributes = new Dictionary<string, MessageAttribute>();
                foreach (var attr in request.MessageAttributes)
                {
                    messageAttributes[attr.Key] = new MessageAttribute
                    {
                        DataType = attr.Value.DataType ?? "String",
                        StringValue = attr.Value.StringValue,
                        BinaryValue = attr.Value.BinaryValue
                    };
                }
            }

            var capturedMessage = new CapturedSnsMessage
            {
                Id = messageId,
                TopicArn = request.TopicArn,
                Message = request.Message,
                Subject = request.Subject,
                MessageStructure = request.MessageStructure,
                MessageDeduplicationId = request.MessageDeduplicationId,
                MessageGroupId = request.MessageGroupId,
                MessageAttributes = messageAttributes,
                Timestamp = DateTimeOffset.UtcNow,
                RawPayload = body
            };

            // Store the message
            var result = _messageStore.Add(capturedMessage);
            if (!result.Success)
            {
                _logger.LogError("Failed to store message: {Error}", result.Exception?.Message);
                return Task.FromResult<IActionResult>(StatusCode(500, new
                {
                    __type = "InternalError",
                    message = "An error occurred while processing the request"
                }));
            }

            return Task.FromResult<IActionResult>(Ok(new PublishResponse { MessageId = messageId }));
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse request body");
            return Task.FromResult<IActionResult>(BadRequest(new
            {
                __type = "InvalidParameterValue",
                message = "Invalid JSON in request body"
            }));
        }
    }

    private Task<IActionResult> HandleCreateTopic(string body)
    {
        try
        {
            var request = JsonSerializer.Deserialize<CreateTopicRequest>(body);
            if (request == null || string.IsNullOrEmpty(request.Name))
            {
                return Task.FromResult<IActionResult>(BadRequest(new
                {
                    __type = "InvalidParameter",
                    message = "Invalid parameter: Name Reason: Must be specified."
                }));
            }

            // Generate a topic ARN
            var topicArn = $"arn:aws:sns:us-east-1:123456789012:{request.Name}";

            return Task.FromResult<IActionResult>(Ok(new CreateTopicResponse { TopicArn = topicArn }));
        }
        catch (JsonException)
        {
            return Task.FromResult<IActionResult>(BadRequest(new
            {
                __type = "InvalidParameterValue",
                message = "Invalid JSON in request body"
            }));
        }
    }

    // Request/Response DTOs
    private class PublishRequest
    {
        public string? TopicArn { get; set; }
        public string? Message { get; set; }
        public string? Subject { get; set; }
        public string? MessageStructure { get; set; }
        public string? MessageDeduplicationId { get; set; }
        public string? MessageGroupId { get; set; }
        public Dictionary<string, MessageAttributeValue>? MessageAttributes { get; set; }
    }

    private class MessageAttributeValue
    {
        public string? DataType { get; set; }
        public string? StringValue { get; set; }
        public byte[]? BinaryValue { get; set; }
    }

    private class PublishResponse
    {
        public string MessageId { get; set; } = "";
    }

    private class CreateTopicRequest
    {
        public string? Name { get; set; }
    }

    private class CreateTopicResponse
    {
        public string TopicArn { get; set; } = "";
    }
}