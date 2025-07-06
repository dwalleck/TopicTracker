# TopicTracker - Product Requirements Document

## Executive Summary

TopicTracker is a lightweight, developer-focused AWS SNS mocking and testing library for .NET applications. It provides a simple way to capture, inspect, and verify SNS messages during local development and testing, eliminating the need for complex AWS infrastructure or third-party tools like LocalStack for SNS-specific testing scenarios.

## Problem Statement

Developers working with AWS Lambda functions and SNS face several challenges:

1. **Limited Visibility**: When testing Lambda functions locally, it's difficult to see what messages are being sent to SNS without deploying to AWS and checking CloudWatch logs
2. **Complex Setup**: Traditional solutions like LocalStack require Docker, have slower startup times, and add unnecessary complexity for simple SNS testing
3. **Poor Developer Experience**: No easy way to quickly invoke functions with different inputs and immediately see the resulting SNS messages
4. **Test Verification**: Existing tools lack proper APIs for programmatically verifying SNS messages in automated tests

## Goals

### Primary Goals
1. Provide instant visibility into SNS messages sent by Lambda functions during local development
2. Create a reusable .NET library that can be easily integrated into any project
3. Enable both manual debugging and automated testing scenarios
4. Eliminate dependency on Docker/LocalStack for simple SNS testing

### Secondary Goals
1. Support .NET Aspire integration for modern cloud-native development
2. Provide a simple web UI for manual testing and debugging
3. Create a NuGet package for easy distribution and reuse

## Target Users

1. **.NET Developers** building AWS Lambda functions that publish to SNS
2. **QA Engineers** who need to verify SNS message outputs during testing
3. **DevOps Engineers** setting up local development environments
4. **Teams** migrating from LocalStack looking for a simpler alternative

## Key Features

### Core Features

#### 1. SNS Message Capture
- Capture all messages published to SNS topics
- Store messages in-memory with configurable retention limits
- Support for message attributes and metadata
- Thread-safe message storage and retrieval

#### 2. Mock SNS Endpoint
- Implement minimal SNS API compatibility (Publish, CreateTopic, Subscribe)
- Accept standard AWS SDK SNS requests
- Return valid SNS responses to maintain compatibility

#### 3. Message Verification API
- RESTful API for querying captured messages
- Filter by topic, timestamp, or message content
- Support for message clearing between tests
- Get individual messages by ID

#### 4. Test Helper Client
- Programmatic API for test assertions
- Wait for messages with timeout support
- Verify message content and attributes
- Message count verification

### Enhanced Features

#### 5. Developer UI
- Simple web interface to view captured messages
- Real-time updates as messages arrive
- Clear messages functionality
- JSON syntax highlighting for message content

#### 6. Aspire Integration
- First-class support for .NET Aspire projects
- Easy service registration and configuration
- Automatic endpoint discovery

#### 7. Statistics and Monitoring
- Message count by topic
- Messages per minute metrics
- Total message statistics

## Technical Requirements

### Technology Stack
- **.NET 8.0+** - Target framework
- **ASP.NET Core** - Web API and hosting
- **TUnit** - Unit testing framework (as specified)
- **Polly** - Resilience patterns (if needed)
- **Tethys.Results** - Railway-oriented programming
- **Source Generators** - For compile-time optimizations and code generation (if beneficial)

### Integration Requirements
- Compatible with AWS SDK for .NET
- Support for standard SNS API operations
- Configuration via environment variables
- Support for both HTTP and HTTPS endpoints

### Performance Requirements
- **High Performance** - Primary design consideration
- Sub-millisecond message capture (target: < 100μs)
- Zero-allocation hot paths where possible
- Support for 10,000+ messages/second throughput
- Minimal memory allocations per message
- Fast message retrieval with indexed lookups
- Optimized JSON serialization/deserialization
- Lock-free operations where feasible

## User Scenarios

### Scenario 1: Local Lambda Development
```
As a developer working on a Lambda function
I want to see SNS messages sent by my function
So that I can debug and verify my implementation
```

**Flow:**
1. Developer starts TopicTracker alongside their Lambda function
2. Invokes Lambda with test data
3. Opens TopicTracker UI to see captured messages
4. Modifies Lambda code based on output
5. Re-tests with immediate feedback

### Scenario 2: Automated Testing
```
As a test engineer
I want to programmatically verify SNS messages
So that I can create reliable automated tests
```

**Flow:**
1. Test setup clears any existing messages
2. Test invokes Lambda function
3. Test uses TopicTracker client to wait for expected message
4. Test verifies message content and attributes
5. Test passes/fails based on verification

### Scenario 3: Team Collaboration
```
As a development team
I want a reusable SNS testing solution
So that all team members have consistent testing capabilities
```

**Flow:**
1. Team adds TopicTracker NuGet package to project
2. Configures TopicTracker in test harness
3. All developers can test SNS locally
4. CI/CD pipeline uses same tool for verification

## Configuration

### Basic Configuration
```csharp
services.AddSnsCapture(options =>
{
    options.Port = 5001;
    options.AutoSubscribe = true;
    options.TopicArns = new List<string> 
    { 
        "arn:aws:sns:us-east-1:000000000000:my-topic" 
    };
});
```

### Environment Variables
- `SNSPY_PORT` - Port for SNS mock endpoint (default: 5001)
- `SNSPY_UI_ENABLED` - Enable/disable web UI (default: true)
- `SNSPY_MAX_MESSAGES` - Maximum messages to store (default: 1000)

## Success Metrics

1. **Developer Productivity**
   - Reduce time to debug SNS issues by 80%
   - Enable testing without AWS deployment

2. **Adoption**
   - 100+ downloads in first month
   - Positive feedback from development teams

3. **Reliability**
   - Zero message loss during capture
   - 99.9% API availability during testing

## Out of Scope

1. Full AWS SNS API compatibility (only essential operations)
2. Message persistence across restarts
3. Production use (development/testing only)
4. Multi-region support
5. SNS subscription delivery (only capture)

## Future Enhancements

1. **Message Replay** - Ability to replay captured messages
2. **Export Functionality** - Export messages to JSON/CSV
3. **Advanced Filtering** - Complex query capabilities
4. **Performance Metrics** - Detailed timing information
5. **Docker Image** - Standalone container option

## Risks and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| AWS SDK compatibility changes | High | Maintain minimal API surface, version testing |
| Memory usage with high message volume | Medium | Configurable retention, automatic cleanup |
| Port conflicts in development | Low | Configurable ports, clear error messages |

## Dependencies

- AWS SDK for .NET (for type definitions)
- ASP.NET Core (for web hosting)
- No external service dependencies
- No Docker requirement

## Timeline

### Phase 1: Core (Week 1-2)
- Basic message capture
- Mock SNS endpoint
- Simple query API

### Phase 2: Testing (Week 3)
- Test helper client
- Verification methods
- Unit tests

### Phase 3: UI & Polish (Week 4)
- Web UI
- Documentation
- NuGet package

### Phase 4: Integration (Week 5)
- Aspire support
- Example projects
- Performance optimization