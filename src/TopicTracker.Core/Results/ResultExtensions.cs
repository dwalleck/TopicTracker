using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tethys.Results;

namespace TopicTracker.Core.Results;

/// <summary>
/// Extension methods for Result types to add TopicTracker-specific functionality.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Result to an appropriate IActionResult for ASP.NET Core controllers.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="errorMapper">Optional custom error mapper for specific error types.</param>
    /// <returns>An appropriate IActionResult based on the result status and error type.</returns>
    public static IActionResult ToActionResult(this Result result, 
        Func<Exception?, IActionResult>? errorMapper = null)
    {
        if (result.Success)
        {
            return new OkResult();
        }
        
        if (errorMapper != null && result.Exception != null)
        {
            return errorMapper(result.Exception);
        }
        
        return result.Exception switch
        {
            ValidationError validationError => new BadRequestObjectResult(new 
            { 
                error = validationError.Message,
                fields = validationError.Errors
            }),
            NotFoundError notFoundError => new NotFoundObjectResult(new 
            { 
                error = notFoundError.Message,
                resourceType = notFoundError.ResourceType,
                resourceId = notFoundError.ResourceId
            }),
            OperationError operationError => new ObjectResult(new 
            { 
                error = operationError.Message,
                operation = operationError.OperationName
            }) 
            { 
                StatusCode = 500 
            },
            _ => new BadRequestObjectResult(new { error = result.Message })
        };
    }
    
    /// <summary>
    /// Converts a Result&lt;T&gt; to an appropriate IActionResult for ASP.NET Core controllers.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="errorMapper">Optional custom error mapper for specific error types.</param>
    /// <returns>An appropriate IActionResult based on the result status and error type.</returns>
    public static IActionResult ToActionResult<T>(this Result<T> result,
        Func<Exception?, IActionResult>? errorMapper = null)
    {
        if (result.Success)
        {
            return new OkObjectResult(result.Data);
        }
        
        if (errorMapper != null && result.Exception != null)
        {
            return errorMapper(result.Exception);
        }
        
        return result.Exception switch
        {
            ValidationError validationError => new BadRequestObjectResult(new 
            { 
                error = validationError.Message,
                fields = validationError.Errors
            }),
            NotFoundError notFoundError => new NotFoundObjectResult(new 
            { 
                error = notFoundError.Message,
                resourceType = notFoundError.ResourceType,
                resourceId = notFoundError.ResourceId
            }),
            OperationError operationError => new ObjectResult(new 
            { 
                error = operationError.Message,
                operation = operationError.OperationName
            }) 
            { 
                StatusCode = 500 
            },
            _ => new BadRequestObjectResult(new { error = result.Message })
        };
    }
    
    /// <summary>
    /// Adds context information to a result.
    /// </summary>
    /// <param name="result">The result to add context to.</param>
    /// <param name="context">The context to add.</param>
    /// <returns>A new result with context information.</returns>
    public static ResultWithContext<object> WithContext(this Result result, object context)
    {
        return new ResultWithContext<object>(result, context);
    }
    
    /// <summary>
    /// Adds context information to a generic result.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to add context to.</param>
    /// <param name="context">The context to add.</param>
    /// <returns>A new result with context information.</returns>
    public static ResultWithContext<object> WithContext<T>(this Result<T> result, object context)
    {
        return new ResultWithContext<object>(
            result.Success ? Result.Ok(result.Message) : Result.Fail(result.Message, result.Exception),
            context
        );
    }
    
    /// <summary>
    /// Validates multiple conditions and returns a single result.
    /// </summary>
    /// <param name="validations">The validation functions to execute.</param>
    /// <returns>A success result if all validations pass, or a failure with aggregated errors.</returns>
    public static Result ValidateAll(params Func<Result>[] validations)
    {
        var errors = new List<ValidationError>();
        
        foreach (var validation in validations)
        {
            var result = validation();
            if (!result.Success && result.Exception is ValidationError validationError)
            {
                errors.Add(validationError);
            }
        }
        
        if (errors.Count == 0)
        {
            return Result.Ok();
        }
        
        return Result.Fail("Validation failed", new AggregatedValidationError(errors));
    }
    
    /// <summary>
    /// Executes an async operation and catches any exceptions, returning a Result.
    /// </summary>
    /// <typeparam name="T">The type of the operation result.</typeparam>
    /// <param name="operation">The async operation to execute.</param>
    /// <param name="errorTransformer">Optional function to transform exceptions into specific error types.</param>
    /// <returns>A Result containing the operation result or error.</returns>
    public static async Task<Result<T>> TryAsync<T>(
        Func<Task<T>> operation,
        Func<Exception, Exception>? errorTransformer = null)
    {
        try
        {
            var result = await operation().ConfigureAwait(false);
            return Result<T>.Ok(result);
        }
        catch (Exception ex)
        {
            var error = errorTransformer?.Invoke(ex) ?? new OperationError("TryAsync", ex.Message, ex);
            return Result<T>.Fail(error.Message, error);
        }
    }
    
    /// <summary>
    /// Executes multiple async operations and returns all results.
    /// </summary>
    /// <typeparam name="T">The type of the operation results.</typeparam>
    /// <param name="tasks">The async operations to execute.</param>
    /// <returns>A Result containing all successful values or an aggregated error.</returns>
    public static async Task<Result<IReadOnlyList<T>>> WhenAllAsync<T>(
        IEnumerable<Task<Result<T>>> tasks)
    {
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        var failures = results.Where(r => !r.Success).ToList();
        
        if (failures.Count == 0)
        {
            return Result<IReadOnlyList<T>>.Ok(results.Select(r => r.Data).ToList());
        }
        
        var errors = failures
            .Where(f => f.Exception != null)
            .Select(f => f.Exception!)
            .ToList();
            
        return Result<IReadOnlyList<T>>.Fail(
            "One or more operations failed", 
            new AggregatedError(errors)
        );
    }
}

/// <summary>
/// Represents a result with additional context information.
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
public class ResultWithContext<TContext>
{
    /// <summary>
    /// Gets the result.
    /// </summary>
    public Result Result { get; }
    
    /// <summary>
    /// Gets the context.
    /// </summary>
    public TContext Context { get; }
    
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success => Result.Success;
    
    /// <summary>
    /// Gets the error if the operation failed.
    /// </summary>
    public Exception? Error => Result.Exception;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultWithContext{TContext}"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="context">The context.</param>
    public ResultWithContext(Result result, TContext context)
    {
        Result = result;
        Context = context;
    }
}