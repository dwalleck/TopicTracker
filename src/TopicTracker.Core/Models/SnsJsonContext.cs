using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TopicTracker.Core.Models;

/// <summary>
/// Source-generated JSON serialization context for high-performance serialization.
/// </summary>
[JsonSerializable(typeof(CapturedSnsMessage))]
[JsonSerializable(typeof(SnsPublishRequest))]
[JsonSerializable(typeof(MessageAttribute))]
[JsonSerializable(typeof(List<CapturedSnsMessage>))]
[JsonSerializable(typeof(Dictionary<string, MessageAttribute>))]
[JsonSerializable(typeof(List<CapturedSnsMessage>), TypeInfoPropertyName = "ListCapturedSnsMessage")]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SnsJsonContext : JsonSerializerContext
{
}