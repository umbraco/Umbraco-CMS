using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

/// <summary>
///     Serializes/deserializes <see cref="ContentCacheDataModel" /> documents to the SQL Database as JSON.
/// </summary>
internal sealed class JsonContentNestedDataSerializer : IContentCacheDataSerializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonObjectConverter(),
        },
    };

    /// <inheritdoc />
    public ContentCacheDataModel? Deserialize(
        IReadOnlyContentBase content,
        string? stringData,
        byte[]? byteData,
        bool published)
    {
        if (stringData == null && byteData != null)
        {
            throw new NotSupportedException(
                $"{typeof(JsonContentNestedDataSerializer)} does not support byte[] serialization");
        }

        return JsonSerializer.Deserialize<ContentCacheDataModel>(stringData!, _jsonSerializerOptions);
    }

    /// <inheritdoc />
    public ContentCacheDataSerializationResult Serialize(
        IReadOnlyContentBase content,
        ContentCacheDataModel model,
        bool published)
    {
        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);
        return new ContentCacheDataSerializationResult(json, null);
    }

    /// <summary>
    /// Provides a converter for handling JSON objects that can be of various types (string, number, boolean, null, or complex types).
    /// </summary>
    internal sealed class JsonObjectConverter : JsonConverter<object>
    {
        /// <inheritdoc/>
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.TryGetInt64(out var value) ? value : reader.GetDouble(),
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Null => null,
                _ => JsonDocument.ParseValue(ref reader).RootElement.Clone(), // fallback for complex types
            };

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
