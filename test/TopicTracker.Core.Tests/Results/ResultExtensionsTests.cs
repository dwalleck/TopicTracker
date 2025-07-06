using Microsoft.AspNetCore.Mvc;
using Tethys.Results;
using TopicTracker.Core.Results;

namespace TopicTracker.Core.Tests.Results;

public class ResultExtensionsTests
{
    [Test]
    public async Task ToActionResult_Should_Return_Ok_For_Success_Result()
    {
        // Arrange
        var result = Result.Ok("Operation successful");
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<OkResult>();
    }
    
    [Test]
    public async Task ToActionResult_Should_Return_BadRequest_For_ValidationError()
    {
        // Arrange
        var validationError = new ValidationError("TopicArn", "TopicArn is required");
        var result = Result.Fail(validationError.Message, validationError);
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<BadRequestObjectResult>();
        
        var badRequest = (BadRequestObjectResult)actionResult;
        await Assert.That(badRequest.Value).IsNotNull();
    }
    
    [Test]
    public async Task ToActionResult_Should_Return_NotFound_For_NotFoundError()
    {
        // Arrange
        var notFoundError = new NotFoundError("Message", "msg-123");
        var result = Result.Fail(notFoundError.Message, notFoundError);
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<NotFoundObjectResult>();
        
        var notFound = (NotFoundObjectResult)actionResult;
        await Assert.That(notFound.Value).IsNotNull();
    }
    
    [Test]
    public async Task ToActionResult_Should_Return_InternalServerError_For_OperationError()
    {
        // Arrange
        var operationError = new OperationError("StoreMessage", "Failed to store message");
        var result = Result.Fail(operationError.Message, operationError);
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<ObjectResult>();
        
        var objectResult = (ObjectResult)actionResult;
        await Assert.That(objectResult.StatusCode).IsEqualTo(500);
    }
    
    [Test]
    public async Task Generic_ToActionResult_Should_Return_Ok_With_Value_For_Success()
    {
        // Arrange
        var message = new { Id = "123", Content = "Test message" };
        var result = Result<object>.Ok(message);
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<OkObjectResult>();
        
        var okResult = (OkObjectResult)actionResult;
        await Assert.That(okResult.Value).IsEqualTo(message);
    }
    
    [Test]
    public async Task Generic_ToActionResult_Should_Return_BadRequest_For_ValidationError()
    {
        // Arrange
        var validationError = new ValidationError("Invalid input");
        var result = Result<string>.Fail(validationError.Message, validationError);
        
        // Act
        var actionResult = result.ToActionResult();
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<BadRequestObjectResult>();
    }
    
    [Test]
    public async Task ToActionResult_Should_Support_Custom_Error_Mapping()
    {
        // Arrange
        var customError = new CustomError("Custom error occurred");
        var result = Result.Fail(customError.Message, customError);
        
        // Act
        var actionResult = result.ToActionResult(error => error switch
        {
            CustomError => new ConflictObjectResult(new { error = error.Message }),
            _ => new BadRequestObjectResult(new { error = error.Message })
        });
        
        // Assert
        await Assert.That(actionResult).IsNotNull();
        await Assert.That(actionResult).IsTypeOf<ConflictObjectResult>();
    }
    
    [Test]
    public async Task WithContext_Should_Add_Context_To_Result()
    {
        // Arrange
        var result = Result.Ok();
        var context = new { RequestId = "req-123", Timestamp = DateTimeOffset.UtcNow };
        
        // Act
        var resultWithContext = result.WithContext(context);
        
        // Assert
        await Assert.That(resultWithContext.Context).IsEqualTo(context);
        await Assert.That(resultWithContext.Success).IsTrue();
    }
    
    [Test]
    public async Task WithContext_Should_Preserve_Error_Information()
    {
        // Arrange
        var error = new ValidationError("Invalid request");
        var result = Result.Fail(error.Message, error);
        var context = new { RequestId = "req-456" };
        
        // Act
        var resultWithContext = result.WithContext(context);
        
        // Assert
        await Assert.That(resultWithContext.Context).IsEqualTo(context);
        await Assert.That(resultWithContext.Success).IsFalse();
        await Assert.That(resultWithContext.Error).IsEqualTo(error);
    }
    
    [Test]
    public async Task ValidateAll_Should_Return_Success_When_All_Validations_Pass()
    {
        // Arrange
        var validations = new Func<Result>[]
        {
            () => Result.Ok(),
            () => Result.Ok(),
            () => Result.Ok()
        };
        
        // Act
        var result = TopicTracker.Core.Results.ResultExtensions.ValidateAll(validations);
        
        // Assert
        await Assert.That(result.Success).IsTrue();
    }
    
    [Test]
    public async Task ValidateAll_Should_Return_Aggregated_Errors_When_Validations_Fail()
    {
        // Arrange
        var validations = new Func<Result>[]
        {
            () => Result.Ok(),
            () => { var e = new ValidationError("Field1", "Field1 is required"); return Result.Fail(e.Message, e); },
            () => { var e = new ValidationError("Field2", "Field2 is invalid"); return Result.Fail(e.Message, e); }
        };
        
        // Act
        var result = TopicTracker.Core.Results.ResultExtensions.ValidateAll(validations);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsTypeOf<AggregatedValidationError>();
        
        var aggregatedError = (AggregatedValidationError)result.Exception!;
        await Assert.That(aggregatedError.Errors).HasCount().EqualTo(2);
    }
    
    // Custom error type for testing
    private class CustomError : Exception
    {
        public CustomError(string message) : base(message)
        {
        }
    }
}