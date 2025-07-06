using System;
using System.Threading.Tasks;
using Tethys.Results;

namespace TopicTracker.Core.Results;

/// <summary>
/// Extension methods for async operations with Result types.
/// </summary>
public static class AsyncResultExtensions
{
    /// <summary>
    /// Chains an async operation on a successful result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="onSuccess">The async function to execute if the result is successful.</param>
    /// <returns>A new result from the chained operation.</returns>
    public static async Task<Result<TOut>> ThenAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> onSuccess)
    {
        if (!result.Success)
        {
            return Result<TOut>.Fail(result.Message, result.Exception);
        }
        
        return await onSuccess(result.Data).ConfigureAwait(false);
    }
    
    /// <summary>
    /// Transforms the value of a successful result asynchronously.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the output value.</typeparam>
    /// <param name="result">The input result.</param>
    /// <param name="mapper">The async transformation function.</param>
    /// <returns>A new result with the transformed value.</returns>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<TOut>> mapper)
    {
        if (!result.Success)
        {
            return Result<TOut>.Fail(result.Message, result.Exception);
        }
        
        try
        {
            var mappedValue = await mapper(result.Data).ConfigureAwait(false);
            return Result<TOut>.Ok(mappedValue, result.Message);
        }
        catch (Exception ex)
        {
            return Result<TOut>.Fail(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Pattern matches on a result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="result">The result to match.</param>
    /// <param name="onSuccess">The async function to execute if successful.</param>
    /// <param name="onFailure">The async function to execute if failed.</param>
    /// <returns>The value from either function.</returns>
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> onSuccess,
        Func<Exception?, Task<TResult>> onFailure)
    {
        if (result.Success)
        {
            return await onSuccess(result.Data).ConfigureAwait(false);
        }
        
        return await onFailure(result.Exception).ConfigureAwait(false);
    }
}