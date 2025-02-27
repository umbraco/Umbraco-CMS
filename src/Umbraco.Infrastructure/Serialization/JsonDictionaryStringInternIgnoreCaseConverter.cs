using System.Text.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts a dictionary with a string key to or from JSON, using the <see cref="StringComparer.OrdinalIgnoreCase" /> comparer and interning the string key when reading.
/// </summary>
/// <typeparam name="TValue">The type of the dictionary value.</typeparam>
public sealed class JsonDictionaryStringInternIgnoreCaseConverter<TValue> : ReadOnlyJsonConverter<Dictionary<string, TValue>>
{
    /// <inheritdoc />
    public override Dictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            // Get key
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString() ?? throw new JsonException();

            // Get value
            reader.Read();
            TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);
            if (value is not null)
            {
                dictionary[string.Intern(propertyName)] = value;
            }
        }

        throw new JsonException();
    }
}
