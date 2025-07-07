# AWS SDK and ASP.NET Core TestHost Compatibility Issue

## Summary
Integration tests using the AWS SDK for .NET with ASP.NET Core's TestHost are failing due to a URI canonicalization conflict. The AWS SDK creates URIs with `DangerousDisablePathAndQueryCanonicalization` for signature verification purposes, but TestHost cannot process these URIs.

## Error Details
```
System.InvalidOperationException: GetComponents() may not be used for Path/Query on a Uri instance created with UriCreationOptions.DangerousDisablePathAndQueryCanonicalization.
   at System.Uri.GetComponents(UriComponents components, UriFormat format)
   at Microsoft.AspNetCore.Http.PathString.FromUriComponent(Uri uri)
   at Microsoft.AspNetCore.TestHost.ClientHandler.<>c__DisplayClass13_0.<SendAsync>b__1(HttpContext context, PipeReader reader)
```

## Environment
- AWS SDK for .NET: 4.0.0.12 (AWSSDK.SimpleNotificationService)
- ASP.NET Core: 9.0.6
- Microsoft.AspNetCore.Mvc.Testing: 9.0.6
- .NET SDK: 9.0.106
- Also reproduced on .NET 10 preview

## Root Cause Analysis

### 1. AWS SDK URI Creation
The AWS SDK creates URIs with special canonicalization rules disabled for security reasons:
- AWS Signature Version 4 requires exact URI path and query string preservation
- The SDK uses `UriCreationOptions.DangerousDisablePathAndQueryCanonicalization` to prevent automatic normalization
- This ensures the signature calculation matches what AWS services expect

### 2. TestHost URI Processing
ASP.NET Core's TestHost tries to parse incoming URIs to construct the HttpContext:
- In `ClientHandler.SendAsync()`, it calls `PathString.FromUriComponent(uri)`
- This internally calls `uri.GetComponents(UriComponents.Path, UriFormat.Unescaped)`
- This method throws when called on URIs created with disabled canonicalization

### 3. The Conflict
- AWS SDK: "Don't touch my URI, I need it exactly as-is for signatures"
- TestHost: "I need to parse your URI to route the request properly"

## Impact
- Cannot write integration tests using real AWS SDK clients with TestHost
- Forces developers to either:
  - Mock AWS SDK at a higher level (less realistic tests)
  - Use actual HTTP endpoints (slower, more complex test setup)
  - Write custom HTTP handlers that bypass the issue

## Reproduction Code
```csharp
[Test]
public async Task Reproduce_AWS_SDK_TestHost_Issue()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var testServer = factory.Server;
    var httpMessageHandler = testServer.CreateHandler();
    
    var snsConfig = new AmazonSimpleNotificationServiceConfig
    {
        ServiceURL = "http://localhost",
        HttpClientFactory = new TestHttpClientFactory(httpMessageHandler)
    };
    
    var snsClient = new AmazonSimpleNotificationServiceClient(
        new BasicAWSCredentials("mock", "mock"), 
        snsConfig);
    
    // Act & Assert - This will throw
    var request = new PublishRequest
    {
        TopicArn = "arn:aws:sns:us-east-1:123456789012:test-topic",
        Message = "Test message"
    };
    
    // This line throws the InvalidOperationException
    await snsClient.PublishAsync(request);
}

private class TestHttpClientFactory : Amazon.Runtime.HttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    
    public TestHttpClientFactory(HttpMessageHandler handler)
    {
        _handler = handler;
    }
    
    public override HttpClient CreateHttpClient(IClientConfig clientConfig)
    {
        return new HttpClient(_handler);
    }
}
```

## Workarounds

### 1. Direct HTTP Testing (Current Approach)
Instead of using AWS SDK, construct HTTP requests manually:
```csharp
var formData = new FormUrlEncodedContent(new[]
{
    new KeyValuePair<string, string>("Action", "Publish"),
    new KeyValuePair<string, string>("TopicArn", "arn:aws:sns:us-east-1:123456789012:test-topic"),
    new KeyValuePair<string, string>("Message", "Test message")
});

var response = await client.PostAsync("/", formData);
```

### 2. Custom Middleware
Create middleware that intercepts and reconstructs the URI before TestHost processes it.

### 3. Real HTTP Server
Use `WebApplication.CreateBuilder()` with Kestrel on a real port instead of TestHost.

## Potential Solutions

### 1. TestHost Enhancement
Modify TestHost to handle URIs with disabled canonicalization:
```csharp
// In ClientHandler.SendAsync
Uri requestUri;
try 
{
    path = PathString.FromUriComponent(request.RequestUri);
}
catch (InvalidOperationException) when (/* detect canonicalization issue */)
{
    // Fallback: manually extract path
    path = new PathString(request.RequestUri.AbsolutePath);
}
```

### 2. AWS SDK Option
Add an option to AWS SDK to use standard URI canonicalization in test scenarios:
```csharp
var config = new AmazonSimpleNotificationServiceConfig
{
    UseTestCompatibleUris = true // Hypothetical new option
};
```

### 3. Adapter Pattern
Create an adapter that sits between AWS SDK and TestHost to handle the URI translation.

## Related Issues
- Similar issues may affect other AWS service SDKs (S3, DynamoDB, etc.)
- May become more prevalent as security requirements tighten
- Could affect other HTTP clients that use similar URI security measures

## Recommendation
For now, we should:
1. Use direct HTTP testing for SNS endpoint tests
2. Document this limitation for other developers
3. Consider contributing a fix to either ASP.NET Core or AWS SDK
4. Monitor for updates that might resolve this issue

## References
- [AWS Signature Version 4](https://docs.aws.amazon.com/general/latest/gr/signature-version-4.html)
- [URI Canonicalization in .NET](https://docs.microsoft.com/en-us/dotnet/api/system.uri)
- [ASP.NET Core TestHost](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)