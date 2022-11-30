using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class ForceInt32Converter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(object) || objectType == typeof(int);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JValue? jsonValue = serializer.Deserialize<JValue>(reader);

        return jsonValue?.Type == JTokenType.Integer
            ? jsonValue.Value<int>()
            : serializer.Deserialize(reader);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}
