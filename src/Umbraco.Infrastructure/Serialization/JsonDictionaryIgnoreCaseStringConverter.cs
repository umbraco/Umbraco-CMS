using System.Text.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts a dictionary with a string key to or from JSON.
/// </summary>
/// <typeparam name="TValue">The type of the dictionary value.</typeparam>
public sealed class JsonDictionaryIgnoreCaseStringConverter<TValue> : ReadOnlyJsonConverter<IDictionary<string, TValue>>
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeof(IDictionary<string, TValue>).IsAssignableFrom(typeToConvert);

    /// <inheritdoc />
    public override IDictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<IDictionary<string, TValue>>(ref reader, options) is IDictionary<string, TValue> dictionary
        ? new Dictionary<string, TValue>(dictionary, StringComparer.OrdinalIgnoreCase)
        : null;
}
