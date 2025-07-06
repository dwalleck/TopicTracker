using System;

namespace TopicTracker.Core.Results;

/// <summary>
/// Represents a validation error in TopicTracker operations.
/// </summary>
public class ValidationError : Exception
{
    /// <summary>
    /// Gets the name of the field that failed validation, if applicable.
    /// </summary>
    public string? FieldName { get; }
    
    /// <summary>
    /// Gets the collection of field-specific validation errors.
    /// </summary>
    public IReadOnlyDictionary<string, string> Errors { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with a message.
    /// </summary>
    /// <param name="message">The validation error message.</param>
    public ValidationError(string message) : base(message)
    {
        Errors = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with a field name and message.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="message">The validation error message.</param>
    public ValidationError(string fieldName, string message) : base(message)
    {
        FieldName = fieldName;
        Errors = new Dictionary<string, string> { { fieldName, message } };
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationError"/> class with multiple field errors.
    /// </summary>
    /// <param name="errors">The collection of field-specific validation errors.</param>
    internal ValidationError(IDictionary<string, string> errors) 
        : base($"Validation failed for {errors.Count} field(s)")
    {
        Errors = new Dictionary<string, string>(errors);
    }
    
    /// <summary>
    /// Creates a validation error from a collection of field-specific errors.
    /// </summary>
    /// <param name="errors">The collection of field-specific validation errors.</param>
    /// <returns>A new validation error containing all field errors.</returns>
    public static ValidationError FromErrors(IDictionary<string, string> errors)
    {
        return new ValidationError(errors);
    }
}

/// <summary>
/// Represents an error when a requested resource is not found.
/// </summary>
public class NotFoundError : Exception
{
    /// <summary>
    /// Gets the type of resource that was not found.
    /// </summary>
    public string? ResourceType { get; }
    
    /// <summary>
    /// Gets the identifier of the resource that was not found.
    /// </summary>
    public string? ResourceId { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundError"/> class with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundError(string message) : base(message)
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundError"/> class with resource information.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The identifier of the resource that was not found.</param>
    public NotFoundError(string resourceType, string resourceId) 
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Represents an error that occurred during a specific operation.
/// </summary>
public class OperationError : Exception
{
    /// <summary>
    /// Gets the name of the operation that failed.
    /// </summary>
    public string OperationName { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationError"/> class.
    /// </summary>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <param name="message">The error message.</param>
    public OperationError(string operationName, string message) : base(message)
    {
        OperationName = operationName;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="OperationError"/> class with an inner exception.
    /// </summary>
    /// <param name="operationName">The name of the operation that failed.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public OperationError(string operationName, string message, Exception innerException) 
        : base(message, innerException)
    {
        OperationName = operationName;
    }
}

/// <summary>
/// Represents an aggregated validation error containing multiple validation failures.
/// </summary>
public class AggregatedValidationError : ValidationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregatedValidationError"/> class.
    /// </summary>
    /// <param name="errors">The collection of validation errors.</param>
    public AggregatedValidationError(IEnumerable<ValidationError> errors) 
        : base(CombineErrors(errors))
    {
    }
    
    private static IDictionary<string, string> CombineErrors(IEnumerable<ValidationError> errors)
    {
        var combined = new Dictionary<string, string>();
        foreach (var error in errors)
        {
            if (error.FieldName != null)
            {
                combined[error.FieldName] = error.Message;
            }
            else
            {
                foreach (var kvp in error.Errors)
                {
                    combined[kvp.Key] = kvp.Value;
                }
            }
        }
        return combined;
    }
}

/// <summary>
/// Represents an aggregated error containing multiple failures.
/// </summary>
public class AggregatedError : AggregateException
{
    /// <summary>
    /// Gets the collection of errors that were aggregated.
    /// </summary>
    public IReadOnlyList<Exception> Errors { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AggregatedError"/> class.
    /// </summary>
    /// <param name="errors">The collection of errors to aggregate.</param>
    public AggregatedError(IEnumerable<Exception> errors) 
        : base("Multiple errors occurred", errors)
    {
        Errors = errors.ToList();
    }
}