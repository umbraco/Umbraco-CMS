using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Serialization
{
    internal class ForceInt32Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (object) || objectType == typeof (int);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonValue = serializer.Deserialize<JValue>(reader);

            return jsonValue.Type == JTokenType.Integer
                ? jsonValue.Value<int>()
                : serializer.Deserialize(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
