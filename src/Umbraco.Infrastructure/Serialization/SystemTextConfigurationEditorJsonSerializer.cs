using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

// TODO: clean up all config editor serializers when we can migrate fully to System.Text.Json
// - move this implementation to ConfigurationEditorJsonSerializer (delete the old implementation)
// - use this implementation as the registered singleton (delete ContextualConfigurationEditorJsonSerializer)
// - reuse the JsonObjectConverter implementation from management API (delete the local implementation - pending V12 branch update)

public class SystemTextConfigurationEditorJsonSerializer : IConfigurationEditorJsonSerializer
{
    private JsonSerializerOptions _jsonSerializerOptions;

    public SystemTextConfigurationEditorJsonSerializer()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // in some cases, configs aren't camel cased in the DB, so we have to resort to case insensitive
            // property name resolving when creating configuration objects (deserializing DB configs)
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
        _jsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        _jsonSerializerOptions.Converters.Add(new JsonObjectConverter());
    }

    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();

    // TODO: reuse the JsonObjectConverter implementation from management API
    private class JsonObjectConverter : System.Text.Json.Serialization.JsonConverter<object>
    {
        public override object Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
            ParseObject(ref reader);

        public override void Write(
            Utf8JsonWriter writer,
            object objectToWrite,
            JsonSerializerOptions options)
        {
            if (objectToWrite is null)
            {
                return;
            }

            // If an object is equals "new object()", Json.Serialize would recurse forever and cause a stack overflow
            // We have no good way of checking if its an empty object
            // which is why we try to check if the object has any properties, and thus will be empty.
            if (objectToWrite.GetType().Name is "Object" && !objectToWrite.GetType().GetProperties().Any())
            {
                writer.WriteStartObject();
                writer.WriteEndObject();
            }
            else
            {
                JsonSerializer.Serialize(writer, objectToWrite, objectToWrite.GetType(), options);
            }
        }

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
}
