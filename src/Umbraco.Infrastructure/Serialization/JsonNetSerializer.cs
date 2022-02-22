using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization
{
    public class JsonNetSerializer : IJsonSerializer
    {
        protected static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            },
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public string Serialize(object? input) => JsonConvert.SerializeObject(input, JsonSerializerSettings);

        public T? Deserialize<T>(string input) => JsonConvert.DeserializeObject<T>(input, JsonSerializerSettings);

        public T? DeserializeSubset<T>(string input, string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var root = Deserialize<JObject>(input);
            var jToken = root?.SelectToken(key);

            return jToken switch
            {
                JArray jArray => jArray.ToObject<T>(),
                JObject jObject => jObject.ToObject<T>(),
                _ => jToken is null ? default : jToken.Value<T>()
            };
        }
    }
}
