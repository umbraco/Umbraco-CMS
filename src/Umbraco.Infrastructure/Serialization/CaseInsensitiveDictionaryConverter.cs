using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class CaseInsensitiveDictionaryConverter<TValue> : JsonConverter<IDictionary<string, TValue>>
{
    // This is a hacky workaround to creating a "read only converter", since System.Text.Json doesn't support it.
    // Taken from https://github.com/dotnet/runtime/issues/46372#issuecomment-1660515178
    private readonly JsonConverter<IDictionary<string, TValue>> _fallbackConverter = (JsonConverter<IDictionary<string, TValue>>)JsonSerializerOptions.Default.GetConverter(typeof(IDictionary<string, TValue>));

    public override IDictionary<string, TValue>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        IDictionary<string, TValue>? defaultDictionary = JsonSerializer.Deserialize<IDictionary<string, TValue>>(ref reader, options);
        return defaultDictionary is null
            ? null
            : new Dictionary<string, TValue>(defaultDictionary, StringComparer.OrdinalIgnoreCase);
    }

    public override void Write(Utf8JsonWriter writer, IDictionary<string, TValue> value, JsonSerializerOptions options) => _fallbackConverter.Write(writer, value, options);
}
