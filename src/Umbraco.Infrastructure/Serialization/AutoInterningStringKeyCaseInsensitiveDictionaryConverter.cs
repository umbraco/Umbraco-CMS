using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class AutoInterningStringKeyCaseInsensitiveDictionaryConverter<TValue> : JsonConverter<IDictionary<string, TValue>>
{
    // This is a hacky workaround to creating a "read only converter", since System.Text.Json doesn't support it.
    // Taken from https://github.com/dotnet/runtime/issues/46372#issuecomment-1660515178
    private readonly JsonConverter<IDictionary<string, TValue>> _fallbackConverter = (JsonConverter<IDictionary<string, TValue>>)JsonSerializerOptions.Default.GetConverter(typeof(IDictionary<string, TValue>));

    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert) => typeof(IDictionary<string, TValue>).IsAssignableFrom(typeToConvert);

    public override Dictionary<string, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            return null;
        }

        var dictionary = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var key = string.Intern(reader.GetString()!);

                    if (reader.Read() is false)
                    {
                        throw new JsonException();
                    }

                    TValue? value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    dictionary[key] = value!;
                    break;
                case JsonTokenType.Comment:
                    break;
                case JsonTokenType.EndObject:
                    return dictionary;
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<string, TValue> value, JsonSerializerOptions options) => _fallbackConverter.Write(writer, value, options);
}
