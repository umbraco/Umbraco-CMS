using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts an object to or from JSON.
/// </summary>
public sealed class JsonObjectConverter : JsonConverter<object>
{
    /// <inheritdoc />
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => ParseObject(ref reader);

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            return;
        }

        // If the value equals an empty object, Json.Serialize would recurse forever and cause a stack overflow
        // We have no good way of checking if its an empty object
        // which is why we try to check if the object has any properties, and thus will be empty.
        Type inputType = value.GetType();
        if (inputType == typeof(object) && inputType.GetProperties().Length == 0)
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
        }
        else
        {
            JsonSerializer.Serialize(writer, value, inputType, options);
        }
    }

    private static object? ParseObject(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var items = new List<object>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                var item = ParseObject(ref reader);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            // if the list of items consists solely of JsonNodes, we are parsing an array of complex objects and should return a JsonArray.
            // otherwise we are parsing an array of simple or mixed types (i.e. an array or strings or integers) and should return an object array.

            if (items.Any() == false)
            {
                // unable to determine the item type - for consistency we have to return null here,
                // otherwise consumers may experience varying result types based on item presence
                return null;
            }

            Type firstType = items.First().GetType();
            if (items.All(i => i.GetType() == firstType) == false)
            {
                // mixed types of objects, return a list of object (must be list because we return list later on as well)
                return items.ToList();
            }

            if (firstType.IsAssignableTo(typeof(JsonNode)))
            {
                // if we only have JSON nodes in the items collection, return them in a JSON array
                return new JsonArray(items.OfType<JsonNode>().ToArray());
            }

            // create, populate and return a generic list of the first item type
            Type listType = typeof(List<>).MakeGenericType(firstType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in items)
            {
                list.Add(item);
            }

            return list;
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
            JsonTokenType.String when reader.TryGetDateTimeOffset(out DateTimeOffset datetime) => datetime,
            JsonTokenType.String when reader.TryGetDateTime(out DateTime datetime) => datetime,
            JsonTokenType.String => reader.GetString(),
            _ => JsonDocument.ParseValue(ref reader).RootElement.Clone()
        };
    }
}
