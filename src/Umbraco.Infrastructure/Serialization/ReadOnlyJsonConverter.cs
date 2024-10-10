using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts an object or value from JSON.
/// </summary>
/// <typeparam name="T">The type of object or value handled by the converter.</typeparam>
public abstract class ReadOnlyJsonConverter<T> : JsonConverter<T>
{
    private readonly JsonConverter<T> _fallbackConverter = (JsonConverter<T>)JsonSerializerOptions.Default.GetConverter(typeof(T));

    /// <inheritdoc />
    public sealed override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        => _fallbackConverter.Write(writer, value, options);
}
