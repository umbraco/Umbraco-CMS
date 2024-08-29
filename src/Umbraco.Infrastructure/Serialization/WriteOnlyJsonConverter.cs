using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts an object or value to JSON.
/// </summary>
/// <typeparam name="T">The type of object or value handled by the converter.</typeparam>
public abstract class WriteOnlyJsonConverter<T> : JsonConverter<T>
{
    private readonly JsonConverter<T> _fallbackConverter = (JsonConverter<T>)JsonSerializerOptions.Default.GetConverter(typeof(T));

    /// <inheritdoc />
    public sealed override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => _fallbackConverter.Read(ref reader, typeToConvert, options);
}
