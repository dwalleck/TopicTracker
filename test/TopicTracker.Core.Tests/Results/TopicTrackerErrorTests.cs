using Tethys.Results;
using TopicTracker.Core.Results;

namespace TopicTracker.Core.Tests.Results;

public class TopicTrackerErrorTests
{
    [Test]
    public async Task ValidationError_Should_Create_With_Message()
    {
        // Arrange
        const string errorMessage = "TopicArn is required";
        
        // Act
        var error = new ValidationError(errorMessage);
        
        // Assert
        await Assert.That(error.Message).IsEqualTo(errorMessage);
        await Assert.That(error.GetType().Name).IsEqualTo("ValidationError");
    }
    
    [Test]
    public async Task ValidationError_Should_Create_With_Field_Name()
    {
        // Arrange
        const string fieldName = "TopicArn";
        const string errorMessage = "TopicArn is required";
        
        // Act
        var error = new ValidationError(fieldName, errorMessage);
        
        // Assert
        await Assert.That(error.FieldName).IsEqualTo(fieldName);
        await Assert.That(error.Message).IsEqualTo(errorMessage);
    }
    
    [Test]
    public async Task ValidationError_Should_Create_From_Multiple_Errors()
    {
        // Arrange
        var errors = new Dictionary<string, string>
        {
            { "TopicArn", "TopicArn is required" },
            { "Message", "Message cannot be empty" }
        };
        
        // Act
        var error = ValidationError.FromErrors(errors);
        
        // Assert
        await Assert.That(error.Errors).HasCount().EqualTo(2);
        await Assert.That(error.Errors["TopicArn"]).IsEqualTo("TopicArn is required");
        await Assert.That(error.Errors["Message"]).IsEqualTo("Message cannot be empty");
    }
    
    [Test]
    public async Task NotFoundError_Should_Create_With_Resource_Type_And_Id()
    {
        // Arrange
        const string resourceType = "Topic";
        const string resourceId = "arn:aws:sns:us-east-1:123456789012:MyTopic";
        
        // Act
        var error = new NotFoundError(resourceType, resourceId);
        
        // Assert
        await Assert.That(error.ResourceType).IsEqualTo(resourceType);
        await Assert.That(error.ResourceId).IsEqualTo(resourceId);
        await Assert.That(error.Message).Contains("Topic");
        await Assert.That(error.Message).Contains(resourceId);
    }
    
    [Test]
    public async Task NotFoundError_Should_Create_With_Custom_Message()
    {
        // Arrange
        const string customMessage = "The requested topic does not exist";
        
        // Act
        var error = new NotFoundError(customMessage);
        
        // Assert
        await Assert.That(error.Message).IsEqualTo(customMessage);
    }
    
    [Test]
    public async Task Result_Should_Fail_With_ValidationError()
    {
        // Arrange
        var validationError = new ValidationError("TopicArn", "TopicArn is required");
        
        // Act
        var result = Result.Fail(validationError.Message, validationError);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsNotNull();
        await Assert.That(result.Exception).IsTypeOf<ValidationError>();
        await Assert.That(((ValidationError)result.Exception!).FieldName).IsEqualTo("TopicArn");
    }
    
    [Test]
    public async Task Result_Should_Fail_With_NotFoundError()
    {
        // Arrange
        var notFoundError = new NotFoundError("Message", "msg-123");
        
        // Act
        var result = Result.Fail(notFoundError.Message, notFoundError);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsNotNull();
        await Assert.That(result.Exception).IsTypeOf<NotFoundError>();
        await Assert.That(((NotFoundError)result.Exception!).ResourceType).IsEqualTo("Message");
    }
    
    [Test]
    public async Task Generic_Result_Should_Fail_With_ValidationError()
    {
        // Arrange
        var validationError = new ValidationError("Invalid request");
        
        // Act
        var result = Result<string>.Fail(validationError.Message, validationError);
        
        // Assert
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Exception).IsNotNull();
        await Assert.That(result.Exception).IsTypeOf<ValidationError>();
    }
    
    [Test]
    public async Task OperationError_Should_Create_With_Operation_Name()
    {
        // Arrange
        const string operationName = "PublishMessage";
        const string errorMessage = "Failed to publish message to topic";
        
        // Act
        var error = new OperationError(operationName, errorMessage);
        
        // Assert
        await Assert.That(error.OperationName).IsEqualTo(operationName);
        await Assert.That(error.Message).IsEqualTo(errorMessage);
    }
    
    [Test]
    public async Task OperationError_Should_Create_With_Inner_Exception()
    {
        // Arrange
        const string operationName = "StoreMessage";
        const string errorMessage = "Failed to store message";
        var innerException = new InvalidOperationException("Storage is full");
        
        // Act
        var error = new OperationError(operationName, errorMessage, innerException);
        
        // Assert
        await Assert.That(error.OperationName).IsEqualTo(operationName);
        await Assert.That(error.Message).IsEqualTo(errorMessage);
        await Assert.That(error.InnerException).IsEqualTo(innerException);
    }
}