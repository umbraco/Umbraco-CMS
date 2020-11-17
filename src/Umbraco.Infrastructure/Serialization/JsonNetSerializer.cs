using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Umbraco.Core.Serialization
{
    public class JsonNetSerializer : IJsonSerializer
    {
        private static readonly JsonConverter[] _defaultConverters = new JsonConverter[]
        {
            new StringEnumConverter()
        };

        public string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input, _defaultConverters);
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, _defaultConverters);
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
