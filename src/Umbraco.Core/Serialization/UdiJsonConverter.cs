using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Serialization
{
    public class UdiJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof (Udi).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JToken.ReadFrom(reader);
            var val = jo.ToObject<string>();
            return val == null ? null : Udi.Parse(val);
        }
    }
}
