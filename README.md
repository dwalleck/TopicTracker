# TopicTracker

[![Progress](https://img.shields.io/badge/dynamic/yaml?url=https://raw.githubusercontent.com/dwalleck/TopicTracker/main/context/TopicTracker/progress.yaml&label=Progress&query=$.metrics.completed_tasks&suffix=/${metrics.total_tasks}%20tasks)](./context/TopicTracker/PROGRESS.md)
[![Phase](https://img.shields.io/badge/dynamic/yaml?url=https://raw.githubusercontent.com/dwalleck/TopicTracker/main/context/TopicTracker/progress.yaml&label=Phase&query=$.current_status.phase_name)](./context/TopicTracker/PROGRESS.md)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance AWS SNS mocking and testing library for .NET applications. TopicTracker provides instant visibility into SNS messages during local development and testing, eliminating the need for complex AWS infrastructure or tools like LocalStack.

## 🚀 Features

- ✅ **Lightning Fast** - <100μs message capture latency
- ✅ **Thread-Safe** - Built for concurrent testing scenarios
- ✅ **TDD-Friendly** - First-class support for Test-Driven Development
- ✅ **Railway-Oriented** - Uses Tethys.Results for robust error handling
- ✅ **.NET Aspire Ready** - Seamless integration with cloud-native applications
- ✅ **Zero Dependencies** - No Docker or external services required
- ✅ **Real-Time Visibility** - See SNS messages instantly during debugging

## 📋 Quick Start

### Installation

```bash
dotnet add package TopicTracker --prerelease
```

### Basic Usage

```csharp
// In your test project
var builder = WebApplication.CreateBuilder(args);

// Add TopicTracker
builder.Services.AddTopicTracker(options =>
{
    options.Port = 5001;
    options.MaxMessages = 1000;
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

### Configure Your Lambda

```csharp
// Point your Lambda to TopicTracker instead of AWS
var snsConfig = new AmazonSNSConfig
{
    ServiceURL = Environment.GetEnvironmentVariable("SNS_ENDPOINT_URL") ?? "http://localhost:5001"
};
var snsClient = new AmazonSNSClient(snsConfig);
```

### Verify Messages in Tests

```csharp
[Test]
public async Task Lambda_Should_Publish_Order_Event()
{
    // Arrange
    var tracker = new TopicTrackerClient("http://localhost:5001");
    await tracker.ClearMessages();
    
    // Act - invoke your Lambda
    await InvokeLambda(new { OrderId = "12345", Amount = 99.99 });
    
    // Assert - verify SNS message was captured
    var published = await tracker.VerifyMessagePublished(
        topicArn: "arn:aws:sns:us-east-1:000000000000:order-events",
        expectedContent: new { OrderId = "12345", Status = "Processed" },
        timeout: TimeSpan.FromSeconds(2)
    );
    
    await Assert.That(published).IsTrue();
}
```

## 🏗️ Architecture

TopicTracker is built with performance and developer experience in mind:

- **In-Memory Storage** - Zero-latency message capture
- **Thread-Safe Collections** - ReaderWriterLockSlim for concurrent access
- **Source Generators** - Compile-time JSON serialization
- **Result Pattern** - Explicit error handling without exceptions

See the [Architecture Documentation](./context/TopicTracker/architecture.md) for detailed design information.

## 🧪 Testing

TopicTracker is built using Test-Driven Development with TUnit:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📊 Development Progress

Track the development progress in real-time:
- [Progress Dashboard](./context/TopicTracker/PROGRESS.md)
- [Development Plan](./context/TopicTracker/development-plan.md)
- [GitHub Project Board](https://github.com/dwalleck/TopicTracker/projects/1)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](./AGENT-GUIDELINES.md) for details on our TDD workflow and quality standards.

### Quick Contribution Guide

1. Pick an issue from the [project board](https://github.com/dwalleck/TopicTracker/projects/1)
2. Write failing tests first (TDD red phase)
3. Implement the minimum code to pass tests
4. Ensure >95% code coverage
5. Submit a PR with your changes

## 📚 Documentation

- [Product Requirements Document](./context/TopicTracker/prd.md)
- [Architecture Document](./context/TopicTracker/architecture.md)
- [Development Plan](./context/TopicTracker/development-plan.md)
- [Agent Guidelines](./AGENT-GUIDELINES.md)

## 🛠️ Technology Stack

- **.NET 8.0+** - Modern .NET platform
- **TUnit** - Next-generation testing framework
- **Tethys.Results** - Railway-oriented programming
- **Polly** - Resilience and transient fault handling
- **Source Generators** - Compile-time optimizations

## 📈 Performance

TopicTracker is designed for extreme performance:

- **Message Capture**: <100μs latency
- **Throughput**: 10,000+ messages/second
- **Memory**: ~1KB per message
- **Zero Allocations**: On hot paths

## 🎯 Use Cases

- **Local Lambda Development** - Test SNS publishing without AWS
- **Integration Testing** - Verify message flow in CI/CD
- **Debugging** - See exactly what your code publishes
- **Load Testing** - Handle high-throughput scenarios

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by the need for better local AWS development tools
- Built with lessons learned from LocalStack usage
- Leverages the power of modern .NET and TDD practices

---

**Note**: TopicTracker is currently in active development. Check the [progress tracker](./context/TopicTracker/PROGRESS.md) for the latest status.