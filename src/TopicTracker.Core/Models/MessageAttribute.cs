namespace TopicTracker.Core.Models;

/// <summary>
/// Represents an SNS message attribute with support for String, Number, and Binary data types.
/// </summary>
public class MessageAttribute
{
    /// <summary>
    /// Gets or sets the data type of the attribute (String, Number, Binary, String.Array, Number.Array).
    /// </summary>
    public required string DataType { get; init; }
    
    /// <summary>
    /// Gets or sets the string value for String and Number data types.
    /// </summary>
    public string? StringValue { get; init; }
    
    /// <summary>
    /// Gets or sets the binary value for Binary data type.
    /// </summary>
    public byte[]? BinaryValue { get; init; }
    
    /// <summary>
    /// Gets the value property for compatibility. Returns StringValue for non-binary types.
    /// </summary>
    public string? Value => StringValue;
}