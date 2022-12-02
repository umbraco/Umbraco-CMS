using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Api.Management.Json;

public class JsonObjectConverter : JsonConverter<object>
{
    public override object Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        ParseObject(ref reader);

    public override void Write(
        Utf8JsonWriter writer,
        object objectToWrite,
        JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);

    private object ParseObject(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var items = new List<object>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                items.Add(ParseObject(ref reader));
            }

            return items.ToArray();
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            var jsonNode = JsonNode.Parse(ref reader);
            if (jsonNode is JsonObject jsonObject)
            {
                return jsonObject;
            }
        }

        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number when reader.TryGetInt32(out int i) => i,
            JsonTokenType.Number when reader.TryGetInt64(out long l) => l,
            JsonTokenType.Number => reader.GetDouble(),
            JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
            JsonTokenType.String => reader.GetString()!,
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };
    }
}
