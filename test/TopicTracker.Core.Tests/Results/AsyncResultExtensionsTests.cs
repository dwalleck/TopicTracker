using Tethys.Results;
using TopicTracker.Core.Results;

namespace TopicTracker.Core.Tests.Results;

public class AsyncResultExtensionsTests
{
    [Test]
    public async Task ThenAsync_Should_Chain_Successful_Operations()
    {
        // Arrange
        var initialValue = "test";
        var result = Result<string>.Ok(initialValue);
        
        // Act
        var chainedResult = await AsyncResultExtensions.ThenAsync(result, async value =>
        {
            await Task.Delay(10);
            return Result<int>.Ok(value.Length);
        });
        
        // Assert
        await Assert.That(chainedResult.Success).IsTrue();
        await Assert.That(chainedResult.Data).IsEqualTo(4);
    }
    
    [Test]
    public async Task ThenAsync_Should_Stop_Chain_On_Failure()
    {
        // Arrange
        var error = new ValidationError("Initial error");
        var result = Result<string>.Fail(error.Message, error);
        var wasExecuted = false;
        
        // Act
        var chainedResult = await AsyncResultExtensions.ThenAsync(result, async value =>
        {
            wasExecuted = true;
            await Task.Delay(10);
            return Result<int>.Ok(value.Length);
        });
        
        // Assert
        await Assert.That(chainedResult.Success).IsFalse();
        await Assert.That(chainedResult.Exception).IsEqualTo(error);
        await Assert.That(wasExecuted).IsFalse();
    }
    
    [Test]
    public async Task MapAsync_Should_Transform_Success_Value()
    {
        // Arrange
        var message = new { Id = "123", Content = "Test" };
        var result = Result<object>.Ok(message);
        
        // Act
        var mappedResult = await AsyncResultExtensions.MapAsync(result, async msg =>
        {
            await Task.Delay(10);
            return new { MessageId = ((dynamic)msg).Id, Length = ((dynamic)msg).Content.Length };
        });
        
        // Assert
        await Assert.That(mappedResult.Success).IsTrue();
        await Assert.That((object?)((dynamic)mappedResult.Data).MessageId).IsEqualTo("123");
        await Assert.That((object?)((dynamic)mappedResult.Data).Length).IsEqualTo(4);
    }
    
    [Test]
    public async Task MapAsync_Should_Preserve_Error()
    {
        // Arrange
        var error = new NotFoundError("Message", "msg-123");
        var result = Result<string>.Fail(error.Message, error);
        
        // Act
        var mappedResult = await AsyncResultExtensions.MapAsync(result, async value =>
        {
            await Task.Delay(10);
            return value.ToUpper();
        });
        
        // Assert
        await Assert.That(mappedResult.Success).IsFalse();
        await Assert.That(mappedResult.Exception).IsEqualTo(error);
    }
    
    [Test]
    public async Task TryAsync_Should_Catch_Exceptions_And_Return_Failure()
    {
        // Arrange
        async Task<string> ThrowingOperation()
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test exception");
        }
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.TryAsync(ThrowingOperation);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsTypeOf<OperationError>();
        await Assert.That(result.Exception!.Message).Contains("Test exception");
    }
    
    [Test]
    public async Task TryAsync_Should_Return_Success_For_Successful_Operation()
    {
        // Arrange
        async Task<string> SuccessfulOperation()
        {
            await Task.Delay(10);
            return "Success";
        }
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.TryAsync(SuccessfulOperation);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).IsEqualTo("Success");
    }
    
    [Test]
    public async Task TryAsync_With_Error_Transformer_Should_Use_Custom_Error()
    {
        // Arrange
        async Task<string> ThrowingOperation()
        {
            await Task.Delay(10);
            throw new ArgumentException("Invalid argument");
        }
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.TryAsync(
            ThrowingOperation,
            ex => new ValidationError("CustomField", ex.Message)
        );
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsTypeOf<ValidationError>();
        await Assert.That(((ValidationError)result.Exception!).FieldName).IsEqualTo("CustomField");
    }
    
    [Test]
    public async Task MatchAsync_Should_Execute_Success_Path()
    {
        // Arrange
        var result = Result<string>.Ok("test");
        var wasSuccessCalled = false;
        var wasFailureCalled = false;
        
        // Act
        var matchResult = await AsyncResultExtensions.MatchAsync(result,
            onSuccess: async value =>
            {
                wasSuccessCalled = true;
                await Task.Delay(10);
                return value.Length;
            },
            onFailure: async error =>
            {
                wasFailureCalled = true;
                await Task.Delay(10);
                return -1;
            }
        );
        
        // Assert
        await Assert.That(matchResult).IsEqualTo(4);
        await Assert.That(wasSuccessCalled).IsTrue();
        await Assert.That(wasFailureCalled).IsFalse();
    }
    
    [Test]
    public async Task MatchAsync_Should_Execute_Failure_Path()
    {
        // Arrange
        var error = new ValidationError("Test error");
        var result = Result<string>.Fail(error.Message, error);
        var wasSuccessCalled = false;
        var wasFailureCalled = false;
        
        // Act
        var matchResult = await AsyncResultExtensions.MatchAsync(result,
            onSuccess: async value =>
            {
                wasSuccessCalled = true;
                await Task.Delay(10);
                return value.Length;
            },
            onFailure: async err =>
            {
                wasFailureCalled = true;
                await Task.Delay(10);
                return -1;
            }
        );
        
        // Assert
        await Assert.That(matchResult).IsEqualTo(-1);
        await Assert.That(wasSuccessCalled).IsFalse();
        await Assert.That(wasFailureCalled).IsTrue();
    }
    
    [Test]
    public async Task WhenAllAsync_Should_Return_Success_When_All_Succeed()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<int>.Ok(2)),
            Task.FromResult(Result<int>.Ok(3))
        };
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.WhenAllAsync(tasks);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Data).HasCount().EqualTo(3);
        await Assert.That(result.Data).Contains(1);
        await Assert.That(result.Data).Contains(2);
        await Assert.That(result.Data).Contains(3);
    }
    
    [Test]
    public async Task WhenAllAsync_Should_Return_Failure_When_Any_Fail()
    {
        // Arrange
        var tasks = new[]
        {
            Task.FromResult(Result<int>.Ok(1)),
            Task.FromResult(Result<int>.Fail("Error 1", new ValidationError("Error 1"))),
            Task.FromResult(Result<int>.Fail("Error 2", new ValidationError("Error 2")))
        };
        
        // Act
        var result = await TopicTracker.Core.Results.ResultExtensions.WhenAllAsync(tasks);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsTypeOf<AggregatedError>();
        
        var aggregatedError = (AggregatedError)result.Exception!;
        await Assert.That(aggregatedError.Errors).HasCount().EqualTo(2);
    }
}