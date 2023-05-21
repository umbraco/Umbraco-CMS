using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class KnownTypeUdiJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(Udi).IsAssignableFrom(objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        writer.WriteValue(value?.ToString());

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jo = JToken.ReadFrom(reader);
        var val = jo.ToObject<string>();
        return val == null ? null : UdiParser.Parse(val, true);
    }
}
