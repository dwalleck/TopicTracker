using System;

namespace TopicTracker.Core.Storage;

/// <summary>
/// Represents an error that occurred in the message store.
/// </summary>
public class MessageStoreError : Exception
{
    /// <summary>
    /// Gets the error code associated with this error.
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageStoreError"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public MessageStoreError(string code, string message) : base(message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageStoreError"/> class with an inner exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this error.</param>
    public MessageStoreError(string code, string message, Exception innerException) 
        : base(message, innerException)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}