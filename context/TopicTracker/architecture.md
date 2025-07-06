# TopicTracker - Architecture Document

## Overview

TopicTracker is designed as a lightweight, in-process SNS mocking service that runs alongside .NET applications. It intercepts AWS SNS SDK calls and provides visibility into published messages through both programmatic and visual interfaces.

## Architecture Principles

1. **High Performance**: Zero-allocation hot paths and optimized data structures
2. **Railway-Oriented Programming**: Using Tethys.Results for error handling
3. **Developer Experience**: Fast feedback loops and easy integration
4. **Testability**: First-class support for automated testing with TUnit
5. **Extensibility**: Clear interfaces for adding features
6. **Compile-Time Optimization**: Leverage source generators where beneficial

## High-Level Architecture

```
┌─────────────────────┐     ┌─────────────────────┐
│   Lambda Function   │     │    Test Harness     │
│  (AWS SDK Client)   │     │  (Manual Testing)   │
└──────────┬──────────┘     └──────────┬──────────┘
           │                           │
           │ SNS Publish Requests      │ HTTP Requests
           │                           │
           ▼                           ▼
┌─────────────────────────────────────────────────┐
│                 TopicTracker Service                    │
├─────────────────────┬───────────────────────────┤
│   Mock SNS API      │    Verification API       │
│   (/sns endpoint)   │    (/api/sns-capture)     │
├─────────────────────┴───────────────────────────┤
│              Message Store (In-Memory)           │
├─────────────────────────────────────────────────┤
│         Core Services & Background Tasks         │
└─────────────────────────────────────────────────┘
           │                           │
           ▼                           ▼
┌─────────────────────┐     ┌─────────────────────┐
│   Test Client SDK   │     │      Web UI         │
│  (NuGet Package)    │     │   (Optional)        │
└─────────────────────┘     └─────────────────────┘
```

## Component Architecture

### 1. Mock SNS Endpoint

**Purpose**: Accepts AWS SNS SDK requests and captures messages

**Components**:
- `SnsMockController` - Handles AWS SDK HTTP requests
- `SnsRequestParser` - Parses AWS API requests
- `SnsResponseBuilder` - Builds AWS-compatible responses

**Key Endpoints**:
- `POST /` - Main SNS API endpoint
- Handles X-Amz-Target header for action routing

### 2. Message Store

**Purpose**: Thread-safe in-memory storage for captured messages

**Components**:
- `SnsCaptureStore` - Core storage implementation
- `CapturedSnsMessage` - Message model
- `ReaderWriterLockSlim` - Concurrency control

**Features**:
- Automatic size limiting (configurable max messages)
- Query capabilities (by topic, time, attributes)
- Thread-safe read/write operations

### 3. Verification API

**Purpose**: RESTful API for querying and managing captured messages

**Components**:
- `SnsMockVerificationController` - REST endpoints
- Query filters and pagination
- Message statistics

**Key Endpoints**:
- `GET /api/sns-capture/messages` - List messages
- `GET /api/sns-capture/messages/{id}` - Get specific message
- `DELETE /api/sns-capture/messages` - Clear all messages
- `POST /api/sns-capture/verify` - Complex verification

### 4. Test Helper Client

**Purpose**: Programmatic interface for test assertions

**Components**:
- `SnsMockClient` - Main client class
- Retry logic with configurable timeouts
- Fluent assertion methods

**Key Methods**:
- `VerifyMessagePublished<T>()` - Wait for and verify message
- `GetMessages()` - Retrieve messages with filters
- `ClearMessages()` - Reset state between tests

### 5. Background Services

**Purpose**: Handle async operations and maintenance

**Components**:
- `SnsCaptureService` - Main hosted service
- HTTP listener for subscription confirmations
- Auto-subscription to configured topics

### 6. Web UI (Optional)

**Purpose**: Visual interface for manual testing

**Components**:
- Minimal HTML/JavaScript UI
- Real-time updates via polling or SSE
- JSON syntax highlighting

## Data Models

### CapturedSnsMessage
```csharp
public class CapturedSnsMessage
{
    public string Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string TopicArn { get; set; }
    public string Message { get; set; }
    public string? Subject { get; set; }
    public Dictionary<string, MessageAttribute>? MessageAttributes { get; set; }
    public string? MessageStructure { get; set; }
    public string RawPayload { get; set; }
}
```

### Configuration Model
```csharp
public class SnsCaptureOptions
{
    public int Port { get; set; } = 5001;
    public string Path { get; set; } = "/sns-capture/";
    public bool AutoSubscribe { get; set; } = true;
    public List<string> TopicArns { get; set; } = new();
    public string CallbackHost { get; set; } = "host.docker.internal";
    public int MaxMessages { get; set; } = 1000;
    public bool EnableUI { get; set; } = true;
}
```

## Integration Patterns

### 1. Direct Integration
```csharp
// In Lambda Function
var snsConfig = new AmazonSNSConfig
{
    ServiceURL = Environment.GetEnvironmentVariable("SNS_ENDPOINT_URL") ?? "https://sns.amazonaws.com",
    UseHttp = true  // For local testing
};
var snsClient = new AmazonSNSClient(new BasicAWSCredentials("mock", "mock"), snsConfig);
```

### 2. Dependency Injection
```csharp
services.AddSingleton<IAmazonSNS>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var snsConfig = new AmazonSNSConfig();
    
    if (config["AWS:UseMockSns"] == "true")
    {
        snsConfig.ServiceURL = config["AWS:MockSnsUrl"];
        snsConfig.UseHttp = true;
    }
    
    return new AmazonSNSClient(snsConfig);
});
```

### 3. Aspire Integration
```csharp
var snsMock = builder.AddProject<Projects.SnsMock>("sns-mock")
    .WithEndpoint(5001, 5001, name: "sns");

var lambda = builder.AddAWSLambdaFunction("processor")
    .WithEnvironment("SNS_ENDPOINT_URL", snsMock.GetEndpoint("sns"));
```

## Thread Safety & Concurrency

### Core Thread Safety Guarantees

```csharp
public class SnsCaptureStore
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<CapturedSnsMessage> _messages = new();
    
    public Result Add(CapturedSnsMessage message)
    {
        _lock.EnterWriteLock();
        try
        {
            _messages.Add(message);
            return Result.Ok("Message stored successfully");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public IReadOnlyList<CapturedSnsMessage> GetMessages()
    {
        _lock.EnterReadLock();
        try
        {
            return _messages.ToList(); // Return copy for thread safety
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
```

### Concurrent Testing with TUnit

```csharp
[Test]
[Repeat(100)]
[ParallelLimit<HighConcurrencyLimit>]
public async Task Concurrent_Message_Storage_Should_Be_ThreadSafe()
{
    var tasks = Enumerable.Range(0, 50)
        .Select(async i => 
        {
            var result = await _store.AddAsync(GenerateMessage(i));
            await Assert.That(result.Success).IsTrue();
        });
    
    await Task.WhenAll(tasks);
    
    // Verify all messages were stored
    await Assert.That(_store.GetMessages().Count).IsEqualTo(50);
}

public class HighConcurrencyLimit : IParallelLimit
{
    public int Limit => Environment.ProcessorCount * 2;
}
```

### Lock-Free Alternatives for High Performance

```csharp
// For extreme performance scenarios, consider lock-free collections
public class LockFreeMessageStore
{
    private readonly ConcurrentBag<CapturedSnsMessage> _messages = new();
    
    public Result Add(CapturedSnsMessage message)
    {
        _messages.Add(message);
        return Result.Ok();
    }
}
```

## Security Considerations

1. **Local Development Only**: Not intended for production use
2. **No Authentication**: Accepts all requests (appropriate for local testing)
3. **No Message Encryption**: Messages stored in plain text
4. **Network Isolation**: Should only bind to localhost by default
5. **Input Validation**: All inputs validated using Result pattern

## Performance Characteristics

### Message Capture
- **Latency**: < 100μs per message capture (target)
- **Throughput**: 10,000+ messages/second
- **Memory**: Optimized per-message allocation
- **Zero-allocation hot path** for message storage

### API Operations
- **Query Performance**: Indexed lookups for common queries
- **Clear Operation**: O(1) - instant clear
- **Verification**: Lock-free reads where possible
- **JSON Processing**: System.Text.Json with source-generated serializers

### Performance Optimizations

#### Source Generators
```csharp
[JsonSerializable(typeof(CapturedSnsMessage))]
[JsonSerializable(typeof(SnsPublishRequest))]
internal partial class SnsJsonContext : JsonSerializerContext { }
```

#### Object Pooling
```csharp
private readonly ObjectPool<CapturedSnsMessage> _messagePool;
```

#### Lock-Free Collections
Consider `ConcurrentQueue<T>` or custom lock-free structures for high-throughput scenarios.

## Deployment Options

### 1. In-Process (Recommended)
- Runs within test harness application
- Shared process with Lambda function
- Fastest performance

### 2. Standalone Service
- Separate ASP.NET Core application
- Can serve multiple Lambda functions
- Requires network configuration

### 3. Container (Future)
- Docker image for platform independence
- Useful for CI/CD environments
- Adds startup overhead

## Extension Points

### 1. Message Processors
Interface for custom message processing:
```csharp
public interface IMessageProcessor
{
    Task ProcessMessage(CapturedSnsMessage message);
}
```

### 2. Storage Providers
Interface for alternative storage:
```csharp
public interface IMessageStore
{
    Task AddMessage(CapturedSnsMessage message);
    Task<IReadOnlyList<CapturedSnsMessage>> GetMessages(MessageQuery query);
    Task Clear();
}
```

### 3. Export Formats
Pluggable exporters for different formats:
```csharp
public interface IMessageExporter
{
    string Format { get; }
    Task<byte[]> Export(IEnumerable<CapturedSnsMessage> messages);
}
```

## Error Handling with Railway-Oriented Programming

### Using Tethys.Results

All operations return `Result` or `Result<T>` types for explicit error handling without exceptions:

```csharp
public Result<CapturedSnsMessage> CaptureMessage(SnsPublishRequest request)
{
    return ValidateRequest(request)
        .Then(() => ParseMessage(request))
        .Then(message => StoreMessage(message))
        .Then(stored => PublishEvent(stored));
}

private Result ValidateRequest(SnsPublishRequest request)
{
    if (string.IsNullOrEmpty(request.TopicArn))
        return Result.Fail("TopicArn is required");
    
    if (string.IsNullOrEmpty(request.Message))
        return Result.Fail("Message is required");
    
    return Result.Ok();
}

private Result<ParsedMessage> ParseMessage(SnsPublishRequest request)
{
    try
    {
        var parsed = JsonSerializer.Deserialize<ParsedMessage>(request.Message);
        return Result<ParsedMessage>.Ok(parsed, "Message parsed successfully");
    }
    catch (JsonException ex)
    {
        return Result<ParsedMessage>.Fail("Failed to parse message", ex);
    }
}
```

### Result Pattern in API Controllers

```csharp
[HttpPost]
public async Task<IActionResult> HandleSnsRequest([FromBody] JsonElement body)
{
    var result = await _snsService.ProcessRequest(body);
    
    return result.Success
        ? Ok(new { MessageId = result.Value })
        : BadRequest(new { Error = result.Message, Details = result.Exception?.Message });
}
```

### Service Layer with Result Chaining

```csharp
public class SnsCaptureService
{
    public async Task<Result<string>> PublishMessage(SnsPublishRequest request)
    {
        // Chain async operations using ThenAsync
        return await ValidateTopicExists(request.TopicArn)
            .ThenAsync(async () => await StoreMessage(request))
            .ThenAsync(async messageId => 
            {
                await _eventPublisher.PublishAsync(new MessageCapturedEvent(messageId));
                return Result<string>.Ok(messageId, "Message published successfully");
            });
    }
    
    private async Task<Result> ValidateTopicExists(string topicArn)
    {
        var exists = await _topicStore.ExistsAsync(topicArn);
        return exists 
            ? Result.Ok() 
            : Result.Fail($"Topic {topicArn} does not exist");
    }
}
```

### Functional Programming with Tethys.Results

#### Pattern Matching with Match

```csharp
public IActionResult ProcessSnsMessage(Result<CapturedSnsMessage> result)
{
    return result.Match(
        onSuccess: message => Ok(new { MessageId = message.Id, Status = "Captured" }),
        onFailure: exception => BadRequest(new { Error = exception.Message })
    );
}

// Async pattern matching
public async Task<IActionResult> ProcessAsync(string messageId)
{
    var result = await _store.GetMessageAsync(messageId);
    
    return await result.MatchAsync(
        onSuccess: async message => 
        {
            await _eventHub.PublishAsync(new MessageViewedEvent(message.Id));
            return Ok(message);
        },
        onFailure: async exception => 
        {
            await _logger.LogErrorAsync(exception);
            return NotFound(new { Error = "Message not found" });
        }
    );
}
```

#### Transforming Values with Map

```csharp
public Result<MessageDto> TransformMessage(Result<CapturedSnsMessage> result)
{
    // Map transforms the value inside a successful Result
    return result.Map(message => new MessageDto
    {
        Id = message.Id,
        Summary = message.Message.Length > 100 
            ? message.Message.Substring(0, 100) + "..." 
            : message.Message,
        TopicName = ExtractTopicName(message.TopicArn),
        FormattedTimestamp = message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
    });
}

// Async mapping for expensive transformations
public async Task<Result<EnrichedMessage>> EnrichMessageAsync(Result<CapturedSnsMessage> result)
{
    return await result.MapAsync(async message =>
    {
        var topicMetadata = await _metadataService.GetTopicMetadataAsync(message.TopicArn);
        return new EnrichedMessage
        {
            Original = message,
            TopicMetadata = topicMetadata,
            ParsedContent = await ParseMessageContentAsync(message.Message)
        };
    });
}
```

#### Chaining Operations with FlatMap

```csharp
public Result<PublishReceipt> ProcessAndForward(SnsPublishRequest request)
{
    // FlatMap chains operations that return Results, avoiding nested Result<Result<T>>
    return ValidateRequest(request)
        .FlatMap(validated => StoreMessage(validated))
        .FlatMap(stored => ForwardToSubscribers(stored))
        .FlatMap(forwarded => GenerateReceipt(forwarded));
}

// Async operation chaining
public async Task<Result<MessageStats>> CalculateStatsAsync(string topicArn)
{
    return await GetTopicAsync(topicArn)
        .FlatMapAsync(async topic => await GetMessagesForTopicAsync(topic))
        .FlatMapAsync(async messages => await CalculateStatisticsAsync(messages))
        .MapAsync(async stats => 
        {
            await _cache.SetAsync($"stats:{topicArn}", stats);
            return stats;
        });
}
```

#### Error Transformation with MapError

```csharp
public Result<CapturedSnsMessage> HandleMessageCapture(SnsPublishRequest request)
{
    return CaptureMessage(request)
        .MapError(exception => exception switch
        {
            JsonException => new ValidationException("Invalid JSON format", exception),
            ArgumentException => new ValidationException("Invalid arguments", exception),
            _ => new ApplicationException("Message capture failed", exception)
        });
}

// Transform errors for external API responses
public async Task<Result<string>> PublishWithRetry(SnsPublishRequest request)
{
    return await PublishAsync(request)
        .MapErrorAsync(async error =>
        {
            await _logger.LogErrorAsync(error);
            
            // Transform internal errors to user-friendly messages
            return error switch
            {
                NetworkException => new Exception("Service temporarily unavailable"),
                ValidationException => error, // Keep validation errors as-is
                _ => new Exception("An error occurred processing your request")
            };
        });
}
```

#### Combining Multiple Results

```csharp
public Result ValidateCompleteMessage(SnsPublishRequest request)
{
    var results = new[]
    {
        ValidateTopicArn(request.TopicArn),
        ValidateMessage(request.Message),
        ValidateAttributes(request.MessageAttributes)
    };
    
    return Result.Combine(results);
}

// Combine with data aggregation
public Result<MessageBatch> ProcessBatch(IEnumerable<SnsPublishRequest> requests)
{
    var results = requests
        .Select(req => ProcessMessage(req))
        .ToList();
    
    var combined = Result<CapturedSnsMessage>.Combine(results);
    
    return combined.Map(messages => new MessageBatch
    {
        Messages = messages,
        TotalCount = messages.Count,
        SuccessCount = messages.Count(m => m != null)
    });
}
```

#### Try Pattern for Exception Handling

```csharp
public Result<ParsedMessage> SafeParseMessage(string json)
{
    return Result<ParsedMessage>.Try(() =>
    {
        var parsed = JsonSerializer.Deserialize<ParsedMessage>(json);
        if (parsed == null)
            throw new InvalidOperationException("Deserialization returned null");
        return parsed;
    });
}

// Async try pattern
public async Task<Result<TopicInfo>> SafeGetTopicInfoAsync(string topicArn)
{
    return await Result<TopicInfo>.TryAsync(async () =>
    {
        var response = await _snsClient.GetTopicAttributesAsync(topicArn);
        return new TopicInfo
        {
            Arn = topicArn,
            Attributes = response.Attributes
        };
    });
}
```

### Error Categories

1. **Invalid Requests**: Return AWS-compatible error responses using Result.Fail
2. **Storage Limits**: Handle gracefully with Result pattern
3. **Port Conflicts**: Clear error messages in Result
4. **Malformed Messages**: Capture exception details in Result

## Monitoring and Diagnostics

### Metrics
- Total messages captured
- Messages per topic
- API request counts
- Memory usage

### Health Checks
```csharp
services.AddHealthChecks()
    .AddCheck<SnsStoreHealthCheck>("sns_store")
    .AddCheck<SnsApiHealthCheck>("sns_api");
```

### Logging
- Structured logging with appropriate levels
- Debug logging for message capture
- Warning for storage limits
- Error for API failures

## Testing Strategy with TUnit

### Unit Tests with TUnit

```csharp
public class SnsCaptureStoreTests
{
    private SnsCaptureStore _store;
    
    [Before(Test)]
    public void Setup()
    {
        _store = new SnsCaptureStore();
    }
    
    [Test]
    public async Task AddMessage_Should_Store_Successfully()
    {
        // Arrange
        var message = new CapturedSnsMessage { Id = "123", Message = "Test" };
        
        // Act
        var result = _store.Add(message);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(_store.GetMessages()).Contains(message);
    }
    
    [Test]
    [Arguments("topic1", 5)]
    [Arguments("topic2", 10)]
    public async Task GetMessagesByTopic_Should_Filter_Correctly(string topicArn, int expectedCount)
    {
        // Arrange
        await GenerateTestMessages(topicArn, expectedCount);
        
        // Act
        var messages = _store.GetMessagesByTopic(topicArn);
        
        // Assert
        await Assert.That(messages.Count).IsEqualTo(expectedCount);
    }
}
```

### Integration Tests

```csharp
[ParallelLimit<SingleThreadedLimit>] // Ensure port isn't shared
public class SnsApiIntegrationTests : IAsyncInitializer
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    
    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }
    
    [Test]
    [Retry(3)] // Retry for stability
    public async Task Full_Message_Flow_Should_Work()
    {
        // Act & Assert with Result pattern
        var publishResult = await PublishTestMessage();
        await Assert.That(publishResult.Success).IsTrue();
        
        var verifyResult = await VerifyMessageCaptured(publishResult.Value);
        await Assert.That(verifyResult.Success).IsTrue();
    }
}
```

### Performance Tests

```csharp
[Test]
[Repeat(100)] // Run multiple times for performance metrics
[ParallelLimit<HighThroughputLimit>]
public async Task High_Throughput_Message_Capture()
{
    var stopwatch = Stopwatch.StartNew();
    var tasks = Enumerable.Range(0, 1000)
        .Select(_ => _store.Add(GenerateMessage()))
        .ToArray();
    
    await Task.WhenAll(tasks);
    stopwatch.Stop();
    
    await Assert.That(stopwatch.ElapsedMilliseconds)
        .IsLessThan(100); // Should handle 1000 messages in < 100ms
}

public class HighThroughputLimit : IParallelLimit
{
    public int Limit => 50; // Allow high concurrency for perf tests
}
```

### Test Data Generation with Source Generators

```csharp
[JsonSerializable(typeof(CapturedSnsMessage))]
[JsonSerializable(typeof(SnsPublishRequest))]
[JsonSerializable(typeof(List<CapturedSnsMessage>))]
internal partial class TestJsonContext : JsonSerializerContext { }

[MethodDataSource(nameof(GenerateTestCases))]
public async Task Parameterized_Tests(TestCase testCase)
{
    // Use source-generated serialization for performance
    var json = JsonSerializer.Serialize(testCase, TestJsonContext.Default.TestCase);
    // Test implementation
}
```

### Test Organization
- Unit tests for isolated components
- Integration tests for end-to-end flows
- Performance benchmarks with TUnit's repeat attribute
- Parallel execution control with custom limits
- Railway-oriented Result assertions

## Development Workflow & Quality Assurance

### Test-Driven Development (TDD) Requirements

All features MUST follow strict TDD practices:

```bash
# 1. Write failing tests first
dotnet test --filter "NewFeatureTests" # Must show failures

# 2. Implement minimal code to pass
# ... implementation ...

# 3. Verify all tests pass
dotnet test --filter "NewFeatureTests" # Must show success

# 4. Check coverage
dotnet test /p:CollectCoverage=true # Must be >95% for new code
```

### Code Quality Gates

#### Automated Quality Checks

```yaml
# CI/CD Pipeline Requirements
- test-coverage:
    threshold: 95% # for new code
- documentation:
    xmlDocs: required # all public APIs
- warnings:
    treatAsErrors: true
- code-analysis:
    rules: TopicTracker.ruleset
```

#### Pre-commit Validation

```csharp
// All public APIs must have XML documentation
/// <summary>
/// Captures and stores an SNS message using railway-oriented error handling.
/// </summary>
/// <param name="request">The SNS publish request to capture.</param>
/// <returns>A Result containing the captured message or error details.</returns>
public Result<CapturedSnsMessage> CaptureMessage(SnsPublishRequest request)
{
    // Implementation following Result pattern
}
```

### Commit Standards

Follow conventional commit format:
- `feat(component): Add new functionality`
- `fix(component): Fix specific issue`
- `test(component): Add/update tests`
- `docs(component): Update documentation`
- `perf(component): Performance improvements`
- `refactor(component): Code improvements`

### Performance Verification

```csharp
[Benchmark]
public void MessageCapture_Performance()
{
    // Benchmark must show <100μs latency
    var result = _store.Add(GenerateMessage());
}

[Benchmark]
public void Serialization_Performance()
{
    // Use source-generated serializers
    var json = JsonSerializer.Serialize(message, SnsJsonContext.Default.CapturedSnsMessage);
}
```

### Documentation Requirements

1. **API Documentation**: All public members must have XML docs
2. **README Updates**: User-facing features must be documented
3. **Architecture Updates**: Significant changes must update this document
4. **Example Code**: New features need usage examples