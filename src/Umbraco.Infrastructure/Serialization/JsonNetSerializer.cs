using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Core.Serialization
{
    public class JsonNetSerializer : IJsonSerializer
    {
        protected static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            }
        };
        public string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input, JsonSerializerSettings);
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, JsonSerializerSettings);
        }

        public T DeserializeSubset<T>(string input, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var root = JsonConvert.DeserializeObject<JObject>(input);

            var jToken = root.SelectToken(key);

            return jToken switch
            {
                JArray jArray => jArray.ToObject<T>(),
                JObject jObject => jObject.ToObject<T>(),
                _ => jToken is null ? default : jToken.Value<T>()
            };
        }
    }
}
