#!/bin/bash
# Script to create comprehensive GitHub issues for TopicTracker development
# Each issue includes detailed context and references to development practices

set -e

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo "GitHub CLI (gh) is required. Install from: https://cli.github.com/"
    exit 1
fi

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "Must be run from within the TopicTracker git repository"
    exit 1
fi

echo "Creating comprehensive GitHub issues for TopicTracker development..."

# Phase 1: Core Foundation Issues

echo "Creating Phase 1: Core Foundation issues..."

gh issue create --title "[FEATURE] Core Data Models" \
    --body "## 📋 Feature Description
Implement core data models for SNS message capture following railway-oriented programming with Tethys.Results.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write failing tests first for all models
- [ ] **CapturedSnsMessage** model with all SNS fields (MessageId, TopicArn, Subject, Message, Timestamp, MessageAttributes)
- [ ] **SnsPublishRequest** model matching AWS SDK structure
- [ ] **MessageAttribute** support with Value, DataType, BinaryValue, StringValue
- [ ] **Source-generated JSON serialization** for performance (<100μs requirement)
- [ ] **Result<T> pattern** for all operations that can fail
- [ ] **Immutable models** with init-only properties
- [ ] **Tests achieve >90% code coverage**
- [ ] **XML documentation** for all public APIs with examples

## 🔧 Technical Details
- **Component**: src/TopicTracker.Core/Models
- **Test Location**: test/TopicTracker.Core.Tests/Models
- **Priority**: P0-Critical
- **Performance Target**: Zero allocations for model creation

## 📚 Development Guidelines
Please follow the practices outlined in:
- **CLAUDE.md**: Test-Driven Development workflow (Red-Green-Refactor)
- **AGENT-GUIDELINES.md**: Section 2.1 (TDD Workflow), Section 4 (Quality Standards)

### TDD Workflow Reminder:
1. Write failing tests first
2. Implement minimum code to pass
3. Refactor while keeping tests green
4. Ensure >90% coverage before marking complete

## 💡 Implementation Hints
\`\`\`csharp
// Example test structure (write this first!)
[Test]
public async Task CapturedSnsMessage_Should_Serialize_To_Json_In_Under_100_Microseconds()
{
    // Arrange
    var message = new CapturedSnsMessage { /* ... */ };
    
    // Act & measure performance
    var stopwatch = Stopwatch.StartNew();
    var json = JsonSerializer.Serialize(message);
    stopwatch.Stop();
    
    // Assert
    await Assert.That(stopwatch.Elapsed.TotalMicroseconds).IsLessThan(100);
}
\`\`\`

## 🔗 References
- [Architecture Document](../context/TopicTracker/architecture.md#data-models)
- [PRD Section 2.2](../context/TopicTracker/prd.md#core-features)" \
    --label "type/feature,component/core,P0-Critical,phase/1-foundation"

gh issue create --title "[FEATURE] Thread-Safe Message Store" \
    --body "## 📋 Feature Description
Implement high-performance, thread-safe in-memory message storage using ReaderWriterLockSlim and Result<T> pattern.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write concurrent access tests first
- [ ] **IMessageStore** interface with Result<T> return types
- [ ] **InMemoryMessageStore** implementation using ReaderWriterLockSlim
- [ ] **Add message** with <100μs latency (performance test required)
- [ ] **Query methods**: by TopicArn, by time range, by MessageId
- [ ] **Auto-cleanup** when message limit reached (LRU eviction)
- [ ] **Thread-safety tests** with 100+ concurrent operations
- [ ] **Memory efficiency**: ~1KB per message overhead
- [ ] **>90% code coverage** including edge cases

## 🔧 Technical Details
- **Component**: src/TopicTracker.Core/Storage
- **Test Location**: test/TopicTracker.Core.Tests/Storage
- **Priority**: P0-Critical
- **Performance Targets**: 
  - Add: <100μs
  - Query: <1ms for 10,000 messages
  - 10,000+ messages/second throughput

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Context preservation, performance requirements
- **AGENT-GUIDELINES.md**: Section 4.7 (Thread Safety), Section 3.2 (Performance Optimization)

### Thread Safety Requirements:
- Use ReaderWriterLockSlim for optimal read performance
- Minimize lock contention
- Avoid deadlocks with proper lock ordering
- Test with ThreadSanitizer or similar tools

## 💡 Implementation Hints
\`\`\`csharp
// Write this test first!
[Test]
public async Task MessageStore_Should_Handle_Concurrent_Adds_Without_Data_Loss()
{
    // Arrange
    var store = new InMemoryMessageStore(maxMessages: 1000);
    var tasks = new List<Task<Result<string>>>();
    
    // Act - 100 concurrent adds
    for (int i = 0; i < 100; i++)
    {
        var message = CreateTestMessage(i);
        tasks.Add(Task.Run(() => store.AddMessage(message)));
    }
    
    var results = await Task.WhenAll(tasks);
    
    // Assert
    await Assert.That(results.All(r => r.IsSuccess)).IsTrue();
    await Assert.That(store.GetMessageCount()).IsEqualTo(100);
}
\`\`\`

## 🏗️ Architecture Considerations
- Consider using ConcurrentDictionary for topic-based indexing
- Implement efficient time-based indexing (SortedSet or custom structure)
- Pre-allocate collections to avoid resizing during operation

## 🔗 References
- [Architecture: Message Storage](../context/TopicTracker/architecture.md#message-storage)
- [Performance Requirements](../context/TopicTracker/prd.md#performance-requirements)" \
    --label "type/feature,component/core,P0-Critical,phase/1-foundation"

gh issue create --title "[FEATURE] Mock SNS Endpoint" \
    --body "## 📋 Feature Description
Create ASP.NET Core endpoint that accepts AWS SDK SNS requests and stores them using IMessageStore.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write integration tests with real AWS SDK first
- [ ] **SNS Controller** accepting POST requests at /
- [ ] **Parse X-Amz-Target** header for action routing
- [ ] **Handle AmazonSNS.Publish** action with Result<T> responses
- [ ] **Handle AmazonSNS.CreateTopic** action
- [ ] **AWS-compatible XML/JSON responses** based on SDK version
- [ ] **Request validation** with meaningful error messages
- [ ] **Integration tests** using actual AWS SNS SDK
- [ ] **Performance**: <1ms response time
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Api/Controllers
- **Test Location**: test/TopicTracker.Api.Tests/Controllers
- **Priority**: P0-Critical
- **Dependencies**: IMessageStore, Tethys.Results

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Railway-oriented programming, error handling
- **AGENT-GUIDELINES.md**: Section 3.1 (API Design), Section 2.1 (TDD)

### AWS SDK Compatibility:
- Test with AWS SDK for .NET v3
- Support both JSON and XML response formats
- Match AWS error response structure
- Validate against real SNS responses

## 💡 Implementation Hints
\`\`\`csharp
// Write this integration test first!
[Test]
public async Task SnsEndpoint_Should_Accept_Real_AWS_SDK_Publish_Request()
{
    // Arrange
    var factory = new WebApplicationFactory<Program>();
    var snsConfig = new AmazonSimpleNotificationServiceConfig
    {
        ServiceURL = factory.Server.BaseAddress.ToString()
    };
    var snsClient = new AmazonSimpleNotificationServiceClient(snsConfig);
    
    // Act
    var response = await snsClient.PublishAsync(new PublishRequest
    {
        TopicArn = \"arn:aws:sns:us-east-1:123456789012:test-topic\",
        Message = \"Test message\"
    });
    
    // Assert
    await Assert.That(response.HttpStatusCode).IsEqualTo(HttpStatusCode.OK);
    await Assert.That(response.MessageId).IsNotNull();
}
\`\`\`

## 🏗️ Railway-Oriented Implementation
\`\`\`csharp
[HttpPost]
public async Task<IActionResult> HandleSnsRequest()
{
    return await ParseSnsAction(Request)
        .FlatMap(action => action switch
        {
            \"AmazonSNS.Publish\" => HandlePublish(Request),
            \"AmazonSNS.CreateTopic\" => HandleCreateTopic(Request),
            _ => Result<IActionResult>.Failure(new NotSupportedException(\$\"Action {action} not supported\"))
        })
        .Match(
            onSuccess: result => result,
            onFailure: error => BadRequest(FormatAwsError(error))
        );
}
\`\`\`

## 🔗 References
- [AWS SNS API Reference](https://docs.aws.amazon.com/sns/latest/api/API_Operations.html)
- [Architecture: API Layer](../context/TopicTracker/architecture.md#api-endpoints)" \
    --label "type/feature,component/api,P0-Critical,phase/1-foundation"

gh issue create --title "[FEATURE] Result Pattern Integration" \
    --body "## 📋 Feature Description
Implement comprehensive Result<T> pattern usage throughout TopicTracker using Tethys.Results.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write tests for failure scenarios first
- [ ] **Common error types** defined (ValidationError, NotFoundError, etc.)
- [ ] **Extension methods** for common patterns (ToActionResult, etc.)
- [ ] **Async support** with Result<Task<T>> patterns
- [ ] **Error aggregation** for batch operations
- [ ] **Logging integration** that preserves error context
- [ ] **Documentation** with usage examples
- [ ] **>95% coverage** of error paths

## 🔧 Technical Details
- **Component**: src/TopicTracker.Core/Results
- **Test Location**: test/TopicTracker.Core.Tests/Results
- **Priority**: P0-Critical
- **Reference**: Tethys.Results patterns

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Railway-oriented programming section
- **AGENT-GUIDELINES.md**: Section 4.5 (Error Handling)
- Review Tethys.Results source for patterns

### Key Patterns to Implement:
1. Validation composition
2. Async operation chaining
3. Error context preservation
4. Batch operation results

## 💡 Implementation Examples
\`\`\`csharp
// Test error scenarios first!
[Test]
public async Task PublishMessage_Should_Return_Failure_When_TopicArn_Invalid()
{
    // Arrange
    var service = new SnsService();
    var request = new PublishRequest { TopicArn = \"invalid-arn\" };
    
    // Act
    var result = await service.PublishMessage(request);
    
    // Assert
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error).IsTypeOf<ValidationError>();
    await Assert.That(result.Error.Message).Contains(\"Invalid topic ARN format\");
}
\`\`\`

## 🏗️ Extension Methods
\`\`\`csharp
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        return result.Match(
            onSuccess: value => new OkObjectResult(value),
            onFailure: error => error switch
            {
                ValidationError => new BadRequestObjectResult(error),
                NotFoundError => new NotFoundObjectResult(error),
                _ => new StatusCodeResult(500)
            }
        );
    }
}
\`\`\`

## 🔗 References
- [Tethys.Results Documentation](https://github.com/TethysResults/Tethys.Results)
- [Architecture: Error Handling](../context/TopicTracker/architecture.md#error-handling)" \
    --label "type/feature,component/core,P0-Critical,phase/1-foundation"

# Phase 2: Testing Infrastructure

echo "Creating Phase 2: Testing Infrastructure issues..."

gh issue create --title "[FEATURE] TopicTracker Test Client" \
    --body "## 📋 Feature Description
Create a test helper client library for easy message verification in tests using Polly for resilience.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write tests for the test client itself
- [ ] **TopicTrackerClient** class with typed methods
- [ ] **GetMessages** with filtering (topic, time range, content)
- [ ] **VerifyMessagePublished** with timeout support
- [ ] **ClearMessages** for test isolation
- [ ] **WaitForMessages** with predicate matching
- [ ] **Polly retry policies** for transient failures
- [ ] **Async/await throughout**
- [ ] **Fluent assertion helpers**
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Client
- **Test Location**: test/TopicTracker.Client.Tests
- **Priority**: P0-Critical
- **NuGet Package**: TopicTracker.Client

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Test infrastructure requirements
- **AGENT-GUIDELINES.md**: Section 2.3 (Testing Best Practices)

### Client Design Principles:
- Developer-friendly API
- Meaningful error messages
- Predictable retry behavior
- Thread-safe operations

## 💡 Implementation Examples
\`\`\`csharp
// Test the test client!
[Test]
public async Task VerifyMessagePublished_Should_Retry_Until_Message_Appears()
{
    // Arrange
    using var app = new TopicTrackerTestApp();
    var client = new TopicTrackerClient(app.BaseAddress);
    
    // Act - publish after delay
    _ = Task.Run(async () =>
    {
        await Task.Delay(500);
        await PublishTestMessage(app);
    });
    
    // Assert - should find message within timeout
    var result = await client.VerifyMessagePublished(
        topicArn: \"test-topic\",
        messageContent: \"delayed message\",
        timeout: TimeSpan.FromSeconds(2)
    );
    
    await Assert.That(result.IsSuccess).IsTrue();
}
\`\`\`

## 🏗️ Fluent API Design
\`\`\`csharp
// Enable fluent assertions
var messages = await client
    .GetMessages()
    .FromTopic(\"arn:aws:sns:us-east-1:123456789012:orders\")
    .WithinLast(TimeSpan.FromMinutes(5))
    .ContainingText(\"order-12345\")
    .Execute();

await Assert.That(messages).HasCount(1);
await Assert.That(messages.First().Message).Contains(\"processed\");
\`\`\`

## 🔗 References
- [Polly Documentation](https://github.com/App-vNext/Polly)
- [Test Client Patterns](../context/TopicTracker/architecture.md#testing-infrastructure)" \
    --label "type/feature,component/client,P0-Critical,phase/2-testing"

gh issue create --title "[FEATURE] TUnit Test Helpers" \
    --body "## 📋 Feature Description
Create TUnit-specific test helpers and custom assertions for TopicTracker testing scenarios.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Test the test helpers
- [ ] **Custom assertions** for SNS message validation
- [ ] **Test fixtures** for common scenarios
- [ ] **Performance assertions** (<100μs, etc.)
- [ ] **Concurrent test helpers**
- [ ] **Message builder fluent API**
- [ ] **AWS SDK mock helpers**
- [ ] **Test data generators**
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Testing
- **Test Location**: test/TopicTracker.Testing.Tests
- **Priority**: P1-High
- **Dependencies**: TUnit, Bogus (for data generation)

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: TUnit testing requirements
- **AGENT-GUIDELINES.md**: Section 2.3 (Testing Patterns)

### TUnit-Specific Features:
- Leverage TUnit's async-first design
- Use TUnit's performance features
- Create reusable test attributes

## 💡 Implementation Examples
\`\`\`csharp
// Custom assertion
public static class SnsAssertions
{
    public static async Task ShouldHaveValidSnsStructure(this CapturedSnsMessage message)
    {
        await Assert.That(message.MessageId).Matches(@\"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}\$\");
        await Assert.That(message.TopicArn).StartsWith(\"arn:aws:sns:\");
        await Assert.That(message.Timestamp).IsAfter(DateTime.UtcNow.AddMinutes(-1));
    }
}

// Performance assertion
[Test]
[PerformanceTest(MaxDuration = 100)] // microseconds
public async Task MessageCapture_Should_Complete_Within_100_Microseconds()
{
    // Test implementation
}
\`\`\`

## 🏗️ Message Builder API
\`\`\`csharp
var message = new SnsMessageBuilder()
    .WithTopic(\"orders\")
    .WithSubject(\"Order Confirmation\")
    .WithMessage(new { OrderId = \"12345\", Status = \"Confirmed\" })
    .WithAttribute(\"CustomerId\", \"67890\")
    .Build();
\`\`\`

## 🔗 References
- [TUnit Documentation](https://github.com/thomhurst/TUnit)
- [Testing Best Practices](../context/TopicTracker/architecture.md#testing)" \
    --label "type/feature,component/testing,P1-High,phase/2-testing"

gh issue create --title "[FEATURE] Verification Methods" \
    --body "## 📋 Feature Description
Implement comprehensive message verification methods with flexible matching and timeout support.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write verification failure tests first
- [ ] **Exact match** verification
- [ ] **Partial match** with JSON path support
- [ ] **Regex pattern** matching
- [ ] **Multiple message** verification
- [ ] **Negative verification** (message NOT published)
- [ ] **Custom predicates** support
- [ ] **Detailed failure messages**
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Client/Verification
- **Test Location**: test/TopicTracker.Client.Tests/Verification
- **Priority**: P1-High

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Error handling and diagnostics
- **AGENT-GUIDELINES.md**: Section 4.6 (Diagnostics)

### Verification Design:
- Clear failure reasons
- Capture actual vs expected
- Support debugging
- Performance considerations

## 💡 Implementation Examples
\`\`\`csharp
// Test failure scenarios first
[Test]
public async Task VerifyMessagePublished_Should_Provide_Helpful_Error_When_No_Match()
{
    // Arrange
    var client = new TopicTrackerClient(baseUrl);
    await client.ClearMessages();
    
    // Publish different message
    await PublishMessage(\"actual message\");
    
    // Act
    var result = await client.VerifyMessagePublished(
        topicArn: \"test-topic\",
        expectedContent: \"expected message\",
        timeout: TimeSpan.FromSeconds(1)
    );
    
    // Assert
    await Assert.That(result.IsFailure).IsTrue();
    await Assert.That(result.Error.Message).Contains(\"Expected: 'expected message'\");
    await Assert.That(result.Error.Message).Contains(\"Actual messages found: 1\");
    await Assert.That(result.Error.Message).Contains(\"actual message\");
}
\`\`\`

## 🏗️ Flexible Matching
\`\`\`csharp
// JSON path matching
await client.VerifyMessagePublished(
    topicArn: \"orders\",
    jsonPath: \"$.OrderId\",
    expectedValue: \"12345\"
);

// Custom predicate
await client.VerifyMessagePublished(
    topicArn: \"orders\",
    predicate: msg => 
    {
        var order = JsonSerializer.Deserialize<Order>(msg.Message);
        return order.Total > 100 && order.Items.Any(i => i.SKU == \"ABC123\");
    }
);
\`\`\`

## 🔗 References
- [Verification Patterns](../context/TopicTracker/architecture.md#verification)
- [PRD: Developer Experience](../context/TopicTracker/prd.md#developer-experience)" \
    --label "type/feature,component/client,P1-High,phase/2-testing"

# Phase 3: API & Integration

echo "Creating Phase 3: API & Integration issues..."

gh issue create --title "[FEATURE] Query Endpoints" \
    --body "## 📋 Feature Description
Implement RESTful query endpoints for retrieving and filtering captured messages.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write API tests first
- [ ] **GET /messages** with pagination
- [ ] **GET /messages/{id}** for specific message
- [ ] **GET /topics** list all seen topics
- [ ] **Query parameters**: topic, startTime, endTime, contains
- [ ] **Sorting options**: time, topic
- [ ] **Response caching** with ETags
- [ ] **OpenAPI documentation**
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Api/Controllers/QueryController.cs
- **Test Location**: test/TopicTracker.Api.Tests/Controllers
- **Priority**: P1-High

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: API design principles
- **AGENT-GUIDELINES.md**: Section 3.1 (RESTful APIs)

### API Design:
- Consistent response format
- Proper HTTP status codes
- HATEOAS links
- Rate limiting consideration

## 💡 Implementation Examples
\`\`\`csharp
// Test query combinations
[Test]
public async Task GetMessages_Should_Filter_By_Multiple_Criteria()
{
    // Arrange
    await SeedTestMessages();
    
    // Act
    var response = await client.GetAsync(
        \"/messages?topic=orders&startTime=2024-01-01T00:00:00Z&contains=processed\"
    );
    
    // Assert
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    var messages = await response.Content.ReadFromJsonAsync<PagedResult<CapturedSnsMessage>>();
    await Assert.That(messages.Items).All(m => 
        m.TopicArn.Contains(\"orders\") && 
        m.Message.Contains(\"processed\") &&
        m.Timestamp >= new DateTime(2024, 1, 1)
    );
}
\`\`\`

## 🏗️ Response Format
\`\`\`json
{
  \"items\": [...],
  \"totalCount\": 150,
  \"pageSize\": 20,
  \"currentPage\": 1,
  \"totalPages\": 8,
  \"_links\": {
    \"self\": \"/messages?page=1&pageSize=20\",
    \"next\": \"/messages?page=2&pageSize=20\",
    \"first\": \"/messages?page=1&pageSize=20\",
    \"last\": \"/messages?page=8&pageSize=20\"
  }
}
\`\`\`

## 🔗 References
- [API Design](../context/TopicTracker/architecture.md#query-api)
- [OpenAPI Specification](https://swagger.io/specification/)" \
    --label "type/feature,component/api,P1-High,phase/3-integration"

gh issue create --title "[FEATURE] Management Endpoints" \
    --body "## 📋 Feature Description
Implement management endpoints for controlling TopicTracker behavior and maintenance.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write management API tests first
- [ ] **DELETE /messages** - clear all messages
- [ ] **DELETE /messages/{id}** - delete specific message
- [ ] **GET /stats** - performance and usage statistics
- [ ] **GET /health** - detailed health check
- [ ] **POST /config** - runtime configuration
- [ ] **Authentication** for management endpoints
- [ ] **Audit logging** for all operations
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Api/Controllers/ManagementController.cs
- **Test Location**: test/TopicTracker.Api.Tests/Controllers
- **Priority**: P2-Medium

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Security considerations
- **AGENT-GUIDELINES.md**: Section 4.8 (Security)

### Security Requirements:
- API key authentication minimum
- Rate limiting on destructive operations
- Audit trail with timestamp and identity
- Configurable authorization policies

## 💡 Implementation Examples
\`\`\`csharp
// Test authorization
[Test]
public async Task DeleteMessages_Should_Require_Authentication()
{
    // Arrange
    var client = CreateClient();
    
    // Act - no auth header
    var response = await client.DeleteAsync(\"/messages\");
    
    // Assert
    await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
}

[Test]
public async Task Stats_Should_Return_Performance_Metrics()
{
    // Arrange
    await GenerateHighLoad();
    
    // Act
    var stats = await client.GetFromJsonAsync<ServiceStats>(\"/stats\");
    
    // Assert
    await Assert.That(stats.AverageLatencyMicroseconds).IsLessThan(100);
    await Assert.That(stats.MessagesPerSecond).IsGreaterThan(1000);
}
\`\`\`

## 🏗️ Stats Response
\`\`\`csharp
public class ServiceStats
{
    public long TotalMessages { get; init; }
    public long TotalTopics { get; init; }
    public double AverageLatencyMicroseconds { get; init; }
    public double MessagesPerSecond { get; init; }
    public long MemoryUsageBytes { get; init; }
    public DateTime StartTime { get; init; }
    public TimeSpan Uptime { get; init; }
}
\`\`\`

## 🔗 References
- [Management API](../context/TopicTracker/architecture.md#management)
- [Security Patterns](../AGENT-GUIDELINES.md#security)" \
    --label "type/feature,component/api,P2-Medium,phase/3-integration"

gh issue create --title "[FEATURE] ASP.NET Core Integration Package" \
    --body "## 📋 Feature Description
Create NuGet package for easy ASP.NET Core integration with dependency injection and configuration.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write integration tests first
- [ ] **Service collection extensions** (AddTopicTracker)
- [ ] **Configuration binding** from appsettings.json
- [ ] **Health checks** integration
- [ ] **Logging integration** with ILogger
- [ ] **Middleware** for correlation IDs
- [ ] **Startup validation**
- [ ] **Sample project** demonstrating usage
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.AspNetCore
- **Test Location**: test/TopicTracker.AspNetCore.Tests
- **Priority**: P1-High
- **NuGet Package**: TopicTracker.AspNetCore

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Integration patterns
- **AGENT-GUIDELINES.md**: Section 3.3 (Dependency Injection)

### Integration Design:
- Zero-config defaults
- Override capabilities
- Validation on startup
- Graceful degradation

## 💡 Implementation Examples
\`\`\`csharp
// Test configuration binding
[Test]
public async Task AddTopicTracker_Should_Bind_Configuration()
{
    // Arrange
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            [\"TopicTracker:MaxMessages\"] = \"5000\",
            [\"TopicTracker:Port\"] = \"5555\"
        })
        .Build();
    
    var services = new ServiceCollection();
    
    // Act
    services.AddTopicTracker(config.GetSection(\"TopicTracker\"));
    var provider = services.BuildServiceProvider();
    var options = provider.GetRequiredService<IOptions<TopicTrackerOptions>>().Value;
    
    // Assert
    await Assert.That(options.MaxMessages).IsEqualTo(5000);
    await Assert.That(options.Port).IsEqualTo(5555);
}
\`\`\`

## 🏗️ Simple Integration
\`\`\`csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add TopicTracker with configuration
builder.Services.AddTopicTracker(options =>
{
    options.MaxMessages = 10000;
    options.EnableMetrics = true;
    options.AuthenticationScheme = \"ApiKey\";
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddTopicTracker();

var app = builder.Build();

// Map endpoints
app.MapTopicTracker(); // Maps all TopicTracker endpoints
app.MapHealthChecks(\"/health\");

app.Run();
\`\`\`

## 🔗 References
- [ASP.NET Core Integration](https://docs.microsoft.com/aspnet/core)
- [DI Best Practices](../context/TopicTracker/architecture.md#dependency-injection)" \
    --label "type/feature,component/integration,P1-High,phase/3-integration"

# Phase 4: Developer Experience

echo "Creating Phase 4: Developer Experience issues..."

gh issue create --title "[FEATURE] Web UI Dashboard" \
    --body "## 📋 Feature Description
Create a web-based dashboard for viewing and searching captured SNS messages in real-time.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write UI component tests first
- [ ] **Message list** with real-time updates
- [ ] **Search and filter** capabilities
- [ ] **Message details** view with JSON formatting
- [ ] **Topic statistics** visualization
- [ ] **Export functionality** (CSV, JSON)
- [ ] **Responsive design** for mobile
- [ ] **Dark mode** support
- [ ] **>80% code coverage** for logic

## 🔧 Technical Details
- **Component**: src/TopicTracker.UI
- **Test Location**: test/TopicTracker.UI.Tests
- **Priority**: P2-Medium
- **Tech Stack**: Blazor Server or minimal JS

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: User experience requirements
- **AGENT-GUIDELINES.md**: Section 5 (UI Guidelines)

### UI Design Principles:
- Minimal and fast
- Real-time updates via SignalR/SSE
- Accessibility compliant
- No heavy frameworks if possible

## 💡 Implementation Examples
\`\`\`csharp
// Test real-time updates
[Test]
public async Task MessageList_Should_Update_When_New_Message_Arrives()
{
    // Arrange
    var page = await browser.NewPageAsync();
    await page.GotoAsync(\"http://localhost:5001/ui\");
    
    // Act - publish message via API
    await PublishTestMessage(\"Real-time test\");
    
    // Assert - message appears without refresh
    await page.WaitForSelectorAsync(\"text=Real-time test\", new() { Timeout = 2000 });
    var messageCount = await page.Locator(\".message-row\").CountAsync();
    await Assert.That(messageCount).IsGreaterThan(0);
}
\`\`\`

## 🏗️ Minimal UI Approach
\`\`\`html
<!-- Using HTMX for simplicity -->
<div hx-get=\"/api/messages\" 
     hx-trigger=\"every 2s\" 
     hx-target=\"#message-list\">
    <table id=\"message-list\">
        <!-- Messages rendered here -->
    </table>
</div>
\`\`\`

## 🔗 References
- [UI Requirements](../context/TopicTracker/prd.md#user-interface)
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)" \
    --label "type/feature,component/ui,P2-Medium,phase/4-dx"

gh issue create --title "[FEATURE] CLI Tools" \
    --body "## 📋 Feature Description
Create command-line tools for interacting with TopicTracker from terminal or CI/CD pipelines.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write CLI tests first
- [ ] **topic-tracker** CLI with subcommands
- [ ] **list** - show messages with formatting options
- [ ] **watch** - real-time message monitoring
- [ ] **clear** - remove messages
- [ ] **export** - save messages to file
- [ ] **stats** - show performance metrics
- [ ] **JSON/Table/CSV** output formats
- [ ] **Shell completion** scripts
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Cli
- **Test Location**: test/TopicTracker.Cli.Tests
- **Priority**: P3-Low
- **Framework**: System.CommandLine

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Developer tooling
- **AGENT-GUIDELINES.md**: Section 5.2 (CLI Design)

### CLI Design:
- Intuitive command structure
- Helpful error messages
- Progress indicators
- Pipe-friendly output

## 💡 Implementation Examples
\`\`\`csharp
// Test CLI commands
[Test]
public async Task Watch_Command_Should_Stream_Messages()
{
    // Arrange
    var output = new StringWriter();
    var app = new TopicTrackerCli(output);
    
    // Act - start watching in background
    var watchTask = Task.Run(() => app.Run(\"watch\", \"--topic\", \"orders\"));
    
    // Publish messages
    await PublishTestMessage(\"Order 1\");
    await Task.Delay(100);
    await PublishTestMessage(\"Order 2\");
    
    // Assert - messages appear in output
    await Task.Delay(500);
    var result = output.ToString();
    await Assert.That(result).Contains(\"Order 1\");
    await Assert.That(result).Contains(\"Order 2\");
}
\`\`\`

## 🏗️ CLI Usage Examples
\`\`\`bash
# List recent messages
topic-tracker list --last 10

# Watch messages in real-time
topic-tracker watch --topic orders --format json

# Export messages
topic-tracker export --start 2024-01-01 --format csv > messages.csv

# Show statistics
topic-tracker stats --format table
\`\`\`

## 🔗 References
- [System.CommandLine](https://github.com/dotnet/command-line-api)
- [CLI Best Practices](../context/TopicTracker/architecture.md#cli-tools)" \
    --label "type/feature,component/cli,P3-Low,phase/4-dx"

gh issue create --title "[FEATURE] Development Mode Features" \
    --body "## 📋 Feature Description
Implement special development mode features for enhanced debugging and testing scenarios.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write tests for dev features
- [ ] **Message replay** functionality
- [ ] **Simulated delays** for testing timeouts
- [ ] **Error injection** for resilience testing
- [ ] **Message templates** for quick testing
- [ ] **Bulk message generation**
- [ ] **Performance profiling** endpoints
- [ ] **Debug headers** in responses
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Core/DevMode
- **Test Location**: test/TopicTracker.Core.Tests/DevMode
- **Priority**: P3-Low

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Development tooling
- **AGENT-GUIDELINES.md**: Section 5.3 (Debug Features)

### Dev Mode Design:
- Clearly marked as development only
- Cannot be enabled in production
- Helpful for testing edge cases
- Performance impact acceptable

## 💡 Implementation Examples
\`\`\`csharp
// Test error injection
[Test]
public async Task ErrorInjection_Should_Simulate_Transient_Failures()
{
    // Arrange
    var client = new TopicTrackerClient(baseUrl);
    await client.EnableDevMode();
    await client.SetErrorRate(0.5); // 50% failure rate
    
    // Act - send 10 requests
    var results = new List<Result<string>>();
    for (int i = 0; i < 10; i++)
    {
        var result = await client.PublishMessage(CreateTestMessage());
        results.Add(result);
    }
    
    // Assert - approximately half should fail
    var failures = results.Count(r => r.IsFailure);
    await Assert.That(failures).IsInRange(3, 7);
}
\`\`\`

## 🏗️ Dev Mode API
\`\`\`csharp
// Enable dev mode features
app.UseTopicTrackerDevMode(options =>
{
    options.EnableReplay = true;
    options.EnableErrorInjection = true;
    options.EnablePerformanceProfiling = true;
    options.MessageTemplates = new[]
    {
        \"OrderCreated\",
        \"PaymentProcessed\",
        \"ShipmentDispatched\"
    };
});
\`\`\`

## 🔗 References
- [Development Features](../context/TopicTracker/prd.md#developer-experience)
- [Testing Scenarios](../context/TopicTracker/architecture.md#testing)" \
    --label "type/feature,component/core,P3-Low,phase/4-dx"

# Phase 5: Platform Integration

echo "Creating Phase 5: Platform Integration issues..."

gh issue create --title "[FEATURE] .NET Aspire Resource" \
    --body "## 📋 Feature Description
Create .NET Aspire resource for TopicTracker to enable seamless integration with Aspire applications.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write Aspire integration tests first
- [ ] **IResourceBuilder** implementation
- [ ] **Automatic service discovery**
- [ ] **Health check integration**
- [ ] **Dashboard integration**
- [ ] **Distributed tracing** support
- [ ] **Environment variable** injection
- [ ] **Container support** optional
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Aspire
- **Test Location**: test/TopicTracker.Aspire.Tests
- **Priority**: P1-High
- **NuGet Package**: TopicTracker.Aspire

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Platform integration
- **AGENT-GUIDELINES.md**: Section 6 (.NET Aspire)

### Aspire Integration:
- Follow Aspire conventions
- Support both local and container
- Integrate with Aspire dashboard
- Service discovery automatic

## 💡 Implementation Examples
\`\`\`csharp
// Test Aspire integration
[Test]
public async Task TopicTracker_Should_Register_With_Aspire()
{
    // Arrange
    var builder = DistributedApplication.CreateBuilder();
    
    // Act
    var topicTracker = builder.AddTopicTracker(\"sns-mock\")
        .WithDataVolume()
        .WithHttpEndpoint(port: 5001);
    
    builder.AddProject<Projects.MyApi>(\"api\")
        .WithReference(topicTracker)
        .WithEnvironment(\"SNS_ENDPOINT_URL\", topicTracker);
    
    // Assert - build and verify
    using var app = builder.Build();
    await app.StartAsync();
    
    var endpoint = topicTracker.GetEndpoint(\"http\");
    await Assert.That(endpoint).IsNotNull();
}
\`\`\`

## 🏗️ Aspire Usage
\`\`\`csharp
// Program.cs in AppHost
var builder = DistributedApplication.CreateBuilder(args);

// Add TopicTracker
var topicTracker = builder.AddTopicTracker(\"topic-tracker\")
    .WithDataVolume()
    .WithHttpEndpoint(name: \"http\");

// Add API that uses TopicTracker
builder.AddProject<Projects.OrderService>(\"order-service\")
    .WithReference(topicTracker)
    .WithEnvironment(\"AWS_SNS_ENDPOINT\", topicTracker.GetEndpoint(\"http\"));

builder.Build().Run();
\`\`\`

## 🔗 References
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire)
- [Aspire Integration Guide](../context/TopicTracker/architecture.md#aspire-integration)" \
    --label "type/feature,component/aspire,P1-High,phase/5-platform"

gh issue create --title "[FEATURE] Aspire Dashboard Integration" \
    --body "## 📋 Feature Description
Integrate TopicTracker metrics and traces with the .NET Aspire Dashboard for observability.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write dashboard integration tests
- [ ] **OpenTelemetry** metrics export
- [ ] **Distributed tracing** with SNS operations
- [ ] **Custom dashboard widgets**
- [ ] **Message flow visualization**
- [ ] **Performance graphs**
- [ ] **Error rate tracking**
- [ ] **Resource health** indicators
- [ ] **>90% code coverage**

## 🔧 Technical Details
- **Component**: src/TopicTracker.Aspire.Dashboard
- **Test Location**: test/TopicTracker.Aspire.Dashboard.Tests
- **Priority**: P2-Medium

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Observability requirements
- **AGENT-GUIDELINES.md**: Section 4.9 (Monitoring)

### Dashboard Integration:
- Use OpenTelemetry standards
- Correlate with application traces
- Meaningful metric names
- Efficient data collection

## 💡 Implementation Examples
\`\`\`csharp
// Test metrics export
[Test]
public async Task TopicTracker_Should_Export_Metrics_To_Aspire()
{
    // Arrange
    var metrics = new List<Metric>();
    using var meterProvider = Sdk.CreateMeterProviderBuilder()
        .AddTopicTrackerInstrumentation()
        .AddInMemoryExporter(metrics)
        .Build();
    
    // Act - generate load
    await PublishMultipleMessages(100);
    
    // Assert - metrics collected
    await Task.Delay(1000); // Allow export
    
    await Assert.That(metrics).Any(m => m.Name == \"topictracker.messages.captured\");
    await Assert.That(metrics).Any(m => m.Name == \"topictracker.latency.microseconds\");
}
\`\`\`

## 🏗️ Metrics Design
\`\`\`csharp
// Key metrics to track
public class TopicTrackerMetrics
{
    private readonly Counter<long> _messagesCaptured;
    private readonly Histogram<double> _captureLatency;
    private readonly ObservableGauge<long> _totalMessages;
    
    public void RecordMessageCaptured(string topicArn, double latencyMicroseconds)
    {
        _messagesCaptured.Add(1, new(\"topic\", topicArn));
        _captureLatency.Record(latencyMicroseconds, new(\"topic\", topicArn));
    }
}
\`\`\`

## 🔗 References
- [OpenTelemetry .NET](https://github.com/open-telemetry/opentelemetry-dotnet)
- [Aspire Dashboard](../context/TopicTracker/architecture.md#observability)" \
    --label "type/feature,component/aspire,P2-Medium,phase/5-platform"

# Phase 6: Production Readiness

echo "Creating Phase 6: Production Readiness issues..."

gh issue create --title "[FEATURE] Performance Optimization" \
    --body "## 📋 Feature Description
Optimize TopicTracker for production workloads meeting <100μs latency and 10,000+ msg/sec targets.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write performance benchmarks first
- [ ] **BenchmarkDotNet** test suite
- [ ] **Memory pool** usage for allocations
- [ ] **Lock-free** data structures where possible
- [ ] **SIMD** optimizations for JSON parsing
- [ ] **Source generators** for serialization
- [ ] **Zero-allocation** hot paths
- [ ] **Performance CI** regression detection
- [ ] **Meet all targets** documented in PRD

## 🔧 Technical Details
- **Component**: src/TopicTracker.Core (optimizations)
- **Test Location**: test/TopicTracker.Benchmarks
- **Priority**: P0-Critical
- **Tools**: BenchmarkDotNet, PerfView

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Performance requirements
- **AGENT-GUIDELINES.md**: Section 3.2 (Performance)

### Optimization Strategy:
1. Measure first (benchmarks)
2. Profile bottlenecks
3. Optimize critical paths
4. Verify improvements
5. Monitor regressions

## 💡 Implementation Examples
\`\`\`csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MessageCaptureBenchmark
{
    private IMessageStore _store;
    private CapturedSnsMessage _message;
    
    [GlobalSetup]
    public void Setup()
    {
        _store = new OptimizedMessageStore();
        _message = CreateTestMessage();
    }
    
    [Benchmark]
    public async Task<Result<string>> CaptureMessage()
    {
        return await _store.AddMessage(_message);
    }
}

// Results should show:
// - Mean: <100μs
// - Allocated: 0 bytes
\`\`\`

## 🏗️ Optimization Techniques
\`\`\`csharp
// Lock-free collection for reads
public class LockFreeMessageStore
{
    private readonly ImmutableList<CapturedSnsMessage> _messages;
    
    // ArrayPool for temporary buffers
    private readonly ArrayPool<byte> _bufferPool;
    
    // Source-generated JSON
    [JsonSerializable(typeof(CapturedSnsMessage))]
    private partial class MessageJsonContext : JsonSerializerContext { }
}
\`\`\`

## 🔗 References
- [Performance Requirements](../context/TopicTracker/prd.md#performance-requirements)
- [BenchmarkDotNet](https://benchmarkdotnet.org)" \
    --label "type/feature,component/performance,P0-Critical,phase/6-production"

gh issue create --title "[FEATURE] NuGet Package & Distribution" \
    --body "## 📋 Feature Description
Create production-ready NuGet packages with proper versioning, signing, and documentation.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write package validation tests
- [ ] **Multi-targeting** (.NET 6.0, 7.0, 8.0)
- [ ] **Package signing** with certificate
- [ ] **Symbol packages** (.snupkg)
- [ ] **README in package**
- [ ] **License file** included
- [ ] **Dependencies minimized**
- [ ] **CI/CD publishing** to NuGet.org
- [ ] **Package validation** tests

## 🔧 Technical Details
- **Component**: All projects
- **Priority**: P0-Critical
- **Packages**: 
  - TopicTracker
  - TopicTracker.Client
  - TopicTracker.AspNetCore
  - TopicTracker.Aspire

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Distribution requirements
- **AGENT-GUIDELINES.md**: Section 7 (Packaging)

### Package Requirements:
- Semantic versioning
- Clear release notes
- Breaking change documentation
- Migration guides

## 💡 Implementation Examples
\`\`\`xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFrameworks>net6.0;net7.0;net8.0</TargetFrameworks>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    
    <!-- Package properties -->
    <Authors>TopicTracker Contributors</Authors>
    <Company>TopicTracker</Company>
    <Product>TopicTracker</Product>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/dwalleck/TopicTracker</PackageProjectUrl>
    <RepositoryUrl>https://github.com/dwalleck/TopicTracker</RepositoryUrl>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageIcon>icon.png</PackageIcon>
    
    <!-- Signing -->
    <SignAssembly>true</SignAssembly>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
\`\`\`

## 🏗️ Package Validation
\`\`\`csharp
[Test]
public async Task Package_Should_Install_And_Work_Correctly()
{
    // Create test project
    var testDir = CreateTempProject();
    
    // Install package
    await RunCommand(\$\"dotnet add package TopicTracker --version {Version}\");
    
    // Build and run
    await RunCommand(\"dotnet build\");
    await RunCommand(\"dotnet run\");
    
    // Verify functionality
    var client = new TopicTrackerClient(\"http://localhost:5001\");
    var health = await client.GetHealth();
    await Assert.That(health.Status).IsEqualTo(\"Healthy\");
}
\`\`\`

## 🔗 References
- [NuGet Best Practices](https://docs.microsoft.com/nuget/create-packages/package-authoring-best-practices)
- [Package Distribution](../context/TopicTracker/development-plan.md#phase-6)" \
    --label "type/feature,component/packaging,P0-Critical,phase/6-production"

gh issue create --title "[FEATURE] Comprehensive Documentation" \
    --body "## 📋 Feature Description
Create complete documentation including getting started, API reference, examples, and troubleshooting.

## 🎯 Acceptance Criteria
- [ ] **TDD Red Phase**: Write doc tests (samples must compile)
- [ ] **Getting Started** guide with examples
- [ ] **API Reference** from XML docs
- [ ] **Integration Guides** for AWS SDK
- [ ] **Performance Tuning** guide
- [ ] **Troubleshooting** section
- [ ] **Sample Projects** (Lambda, ASP.NET, Aspire)
- [ ] **Video Tutorials** (optional)
- [ ] **All code samples tested**

## 🔧 Technical Details
- **Component**: docs/
- **Test Location**: test/TopicTracker.Docs.Tests
- **Priority**: P0-Critical
- **Tools**: DocFX or similar

## 📚 Development Guidelines
Follow practices in:
- **CLAUDE.md**: Documentation standards
- **AGENT-GUIDELINES.md**: Section 8 (Documentation)

### Documentation Standards:
- Clear and concise
- Plenty of examples
- Tested code samples
- Progressive disclosure
- SEO optimized

## 💡 Implementation Examples
\`\`\`csharp
// Test documentation code samples
[Test]
public async Task GettingStarted_CodeSample_Should_Compile_And_Run()
{
    // Extract code from markdown
    var markdown = await File.ReadAllTextAsync(\"docs/getting-started.md\");
    var codeSamples = ExtractCodeBlocks(markdown, \"csharp\");
    
    foreach (var sample in codeSamples)
    {
        // Create test project with sample
        var result = await CompileAndRun(sample);
        await Assert.That(result.Success).IsTrue();
    }
}
\`\`\`

## 🏗️ Documentation Structure
\`\`\`
docs/
├── getting-started/
│   ├── installation.md
│   ├── first-test.md
│   └── aws-lambda.md
├── guides/
│   ├── aspnetcore-integration.md
│   ├── aspire-integration.md
│   ├── performance-tuning.md
│   └── troubleshooting.md
├── api/
│   └── (generated from XML docs)
├── samples/
│   ├── basic-usage/
│   ├── lambda-testing/
│   └── aspire-app/
└── videos/
    └── getting-started.mp4
\`\`\`

## 🔗 References
- [Documentation Plan](../context/TopicTracker/development-plan.md#documentation)
- [DocFX](https://dotnet.github.io/docfx/)" \
    --label "type/documentation,component/docs,P0-Critical,phase/6-production"

# Create meta-issue for tracking

echo "Creating meta tracking issue..."

gh issue create --title "[META] TopicTracker Development Tracker" \
    --body "## 🎯 TopicTracker Development Meta Issue

This issue tracks the overall development progress of TopicTracker, a high-performance AWS SNS mocking service.

## 📋 Development Phases

### Phase 1: Core Foundation (Week 1-2)
- [ ] #1 Core Data Models
- [ ] #2 Thread-Safe Message Store  
- [ ] #3 Mock SNS Endpoint
- [ ] #4 Result Pattern Integration

### Phase 2: Testing Infrastructure (Week 2-3)
- [ ] #5 TopicTracker Test Client
- [ ] #6 TUnit Test Helpers
- [ ] #7 Verification Methods

### Phase 3: API & Integration (Week 3-4)
- [ ] #8 Query Endpoints
- [ ] #9 Management Endpoints
- [ ] #10 ASP.NET Core Integration Package

### Phase 4: Developer Experience (Week 4-5)
- [ ] #11 Web UI Dashboard
- [ ] #12 CLI Tools
- [ ] #13 Development Mode Features

### Phase 5: Platform Integration (Week 5-6)
- [ ] #14 .NET Aspire Resource
- [ ] #15 Aspire Dashboard Integration

### Phase 6: Production Readiness (Week 6-7)
- [ ] #16 Performance Optimization
- [ ] #17 NuGet Package & Distribution
- [ ] #18 Comprehensive Documentation

## 📚 Key Documents
- [Product Requirements](../context/TopicTracker/prd.md)
- [Architecture](../context/TopicTracker/architecture.md)
- [Development Plan](../context/TopicTracker/development-plan.md)
- [Development Guidelines](../AGENT-GUIDELINES.md)

## 🏆 Success Criteria
- [ ] <100μs message capture latency
- [ ] 10,000+ messages/second throughput
- [ ] 90%+ code coverage
- [ ] All quality gates passing
- [ ] Published to NuGet.org
- [ ] Complete documentation

## 📊 Progress Tracking
Progress is automatically tracked at: [Progress Dashboard](../context/TopicTracker/PROGRESS.md)

---
**Note**: Each issue includes detailed context and references to CLAUDE.md and AGENT-GUIDELINES.md to ensure consistent development practices." \
    --label "type/meta,epic"

echo ""
echo "✅ GitHub issues created successfully!"
echo ""
echo "Next steps:"
echo "1. Visit https://github.com/dwalleck/TopicTracker/issues to view all issues"
echo "2. Create a GitHub Project board to organize issues into sprints"
echo "3. Configure automation rules for the project board"
echo "4. Start development following TDD practices!"