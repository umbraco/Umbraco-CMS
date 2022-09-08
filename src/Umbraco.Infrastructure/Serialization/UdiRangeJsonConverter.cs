using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class UdiRangeJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(UdiRange).IsAssignableFrom(objectType);

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        writer.WriteValue(value?.ToString());

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jo = JToken.ReadFrom(reader);
        var val = jo.ToObject<string>();
        return val == null ? null : UdiRange.Parse(val);
    }
}
