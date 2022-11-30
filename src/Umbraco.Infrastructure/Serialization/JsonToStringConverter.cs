using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
///     This is used in order to deserialize a json object on a property into a json string since the property's type is
///     'string'
/// </summary>
internal class JsonToStringConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotImplementedException();

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.ValueType == typeof(string))
        {
            return reader.Value;
        }

        // Load JObject from stream
        var jObject = JObject.Load(reader);
        return jObject.ToString(Formatting.None);
    }

    public override bool CanConvert(Type objectType) => typeof(string).IsAssignableFrom(objectType);
}
