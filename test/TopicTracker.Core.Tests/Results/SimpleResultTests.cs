using Tethys.Results;
using TopicTracker.Core.Results;

namespace TopicTracker.Core.Tests.Results;

public class SimpleResultTests
{
    [Test]
    public async Task ValidationError_Should_Create_With_Message()
    {
        // Arrange & Act
        var error = new ValidationError("TopicArn is required");
        
        // Assert
        await Assert.That(error.Message).IsEqualTo("TopicArn is required");
    }
    
    [Test]
    public async Task NotFoundError_Should_Create_With_Resource_Info()
    {
        // Arrange & Act
        var error = new NotFoundError("Topic", "arn:aws:sns:us-east-1:123456789012:MyTopic");
        
        // Assert
        await Assert.That(error.ResourceType).IsEqualTo("Topic");
        await Assert.That(error.ResourceId).IsEqualTo("arn:aws:sns:us-east-1:123456789012:MyTopic");
    }
    
    [Test]
    public async Task Result_With_ValidationError_Should_Fail()
    {
        // Arrange
        var error = new ValidationError("Invalid input");
        
        // Act
        var result = Result.Fail(error.Message, error);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsTypeOf<ValidationError>();
    }
    
    [Test]
    public async Task ToActionResult_Should_Return_Ok_For_Success()
    {
        // Arrange
        var result = Result.Ok("Success");
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
    }
    
    [Test]
    public async Task TryAsync_Should_Catch_Exceptions()
    {
        // Arrange
        async Task<string> ThrowingOperation()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
        }
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.TryAsync(ThrowingOperation);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsNotNull();
    }
    
    [Test]
    public async Task BatchResult_Should_Track_Operations()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error", new ValidationError("Error")))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(2);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(1);
        await Assert.That(batchResult.FailureCount).IsEqualTo(1);
    }
}