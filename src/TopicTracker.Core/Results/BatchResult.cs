using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tethys.Results;

namespace TopicTracker.Core.Results;

/// <summary>
/// Represents the result of a single operation in a batch.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class BatchOperationResult<T>
{
    /// <summary>
    /// Gets the identifier of the item that was processed.
    /// </summary>
    public string ItemId { get; }
    
    /// <summary>
    /// Gets the result of the operation.
    /// </summary>
    public Result<T> Result { get; }
    
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess => Result.Success;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchOperationResult{T}"/> class.
    /// </summary>
    /// <param name="itemId">The identifier of the item.</param>
    /// <param name="result">The result of the operation.</param>
    public BatchOperationResult(string itemId, Result<T> result)
    {
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        Result = result ?? throw new ArgumentNullException(nameof(result));
    }
}

/// <summary>
/// Represents the results of a batch operation.
/// </summary>
/// <typeparam name="T">The type of the result values.</typeparam>
public class BatchResult<T>
{
    private readonly List<BatchOperationResult<T>> _results;
    
    /// <summary>
    /// Gets the total number of operations in the batch.
    /// </summary>
    public int TotalCount => _results.Count;
    
    /// <summary>
    /// Gets the number of successful operations.
    /// </summary>
    public int SuccessCount => _results.Count(r => r.IsSuccess);
    
    /// <summary>
    /// Gets the number of failed operations.
    /// </summary>
    public int FailureCount => _results.Count(r => !r.IsSuccess);
    
    /// <summary>
    /// Gets a value indicating whether all operations were successful.
    /// </summary>
    public bool IsCompleteSuccess => _results.Count == 0 || _results.All(r => r.IsSuccess);
    
    /// <summary>
    /// Gets a value indicating whether all operations failed.
    /// </summary>
    public bool IsCompleteFailure => _results.Count == 0 || _results.All(r => !r.IsSuccess);
    
    /// <summary>
    /// Gets a value indicating whether any operations failed.
    /// </summary>
    public bool HasFailures => _results.Any(r => !r.IsSuccess);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchResult{T}"/> class.
    /// </summary>
    public BatchResult()
    {
        _results = new List<BatchOperationResult<T>>();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchResult{T}"/> class with results.
    /// </summary>
    /// <param name="results">The batch operation results.</param>
    public BatchResult(IEnumerable<BatchOperationResult<T>> results)
    {
        _results = results?.ToList() ?? new List<BatchOperationResult<T>>();
    }
    
    /// <summary>
    /// Gets all successful operations.
    /// </summary>
    /// <returns>A collection of successful batch operation results.</returns>
    public IEnumerable<BatchOperationResult<T>> GetSuccessfulItems()
    {
        return _results.Where(r => r.IsSuccess);
    }
    
    /// <summary>
    /// Gets all failed operations.
    /// </summary>
    /// <returns>A collection of failed batch operation results.</returns>
    public IEnumerable<BatchOperationResult<T>> GetFailedItems()
    {
        return _results.Where(r => !r.IsSuccess);
    }
    
    /// <summary>
    /// Generates a summary of the batch operation.
    /// </summary>
    /// <returns>A summary object containing statistics about the batch operation.</returns>
    public BatchSummary GenerateSummary()
    {
        var errors = GetFailedItems()
            .Where(item => item.Result.Exception != null)
            .Select(item => new ErrorInfo(item.ItemId, item.Result.Message, item.Result.Exception!))
            .ToList();
            
        return new BatchSummary(
            TotalCount,
            SuccessCount,
            FailureCount,
            errors
        );
    }
    
    /// <summary>
    /// Processes a batch of items asynchronously.
    /// </summary>
    /// <typeparam name="TItem">The type of items to process.</typeparam>
    /// <param name="items">The items to process.</param>
    /// <param name="processor">The function to process each item.</param>
    /// <param name="maxConcurrency">The maximum number of concurrent operations.</param>
    /// <returns>A batch result containing all operation results.</returns>
    public static async Task<BatchResult<T>> ProcessBatchAsync<TItem>(
        IEnumerable<TItem> items,
        Func<TItem, Task<Result<T>>> processor,
        int maxConcurrency = 10)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var results = new List<BatchOperationResult<T>>();
        
        var tasks = items.Select(async (item, index) =>
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var result = await processor(item).ConfigureAwait(false);
                return new BatchOperationResult<T>(item?.ToString() ?? $"item{index}", result);
            }
            finally
            {
                semaphore.Release();
            }
        });
        
        var completedResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        return new BatchResult<T>(completedResults);
    }
}

/// <summary>
/// Represents a summary of a batch operation.
/// </summary>
public class BatchSummary
{
    /// <summary>
    /// Gets the total number of operations.
    /// </summary>
    public int Total { get; }
    
    /// <summary>
    /// Gets the number of successful operations.
    /// </summary>
    public int Successful { get; }
    
    /// <summary>
    /// Gets the number of failed operations.
    /// </summary>
    public int Failed { get; }
    
    /// <summary>
    /// Gets the success rate as a decimal between 0 and 1.
    /// </summary>
    public double SuccessRate => Total > 0 ? (double)Successful / Total : 0;
    
    /// <summary>
    /// Gets the collection of errors that occurred.
    /// </summary>
    public IReadOnlyList<ErrorInfo> Errors { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchSummary"/> class.
    /// </summary>
    /// <param name="total">The total number of operations.</param>
    /// <param name="successful">The number of successful operations.</param>
    /// <param name="failed">The number of failed operations.</param>
    /// <param name="errors">The collection of errors.</param>
    public BatchSummary(int total, int successful, int failed, IReadOnlyList<ErrorInfo> errors)
    {
        Total = total;
        Successful = successful;
        Failed = failed;
        Errors = errors ?? new List<ErrorInfo>();
    }
}

/// <summary>
/// Represents information about an error that occurred during batch processing.
/// </summary>
public class ErrorInfo
{
    /// <summary>
    /// Gets the identifier of the item that caused the error.
    /// </summary>
    public string ItemId { get; }
    
    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Gets the exception that occurred.
    /// </summary>
    public Exception Exception { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorInfo"/> class.
    /// </summary>
    /// <param name="itemId">The identifier of the item.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The exception that occurred.</param>
    public ErrorInfo(string itemId, string message, Exception exception)
    {
        ItemId = itemId;
        Message = message;
        Exception = exception;
    }
}