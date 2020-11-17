using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        public T DeserializeSubset<T>(string input, string value)
        {
            var jObject = JsonConvert.DeserializeObject<dynamic>(input);
            return jObject != null ? jObject.GetValueAsString(value) : input;
        }
    }
}
