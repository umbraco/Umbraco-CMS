using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Serialization
{
    [UmbracoVolatile]
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
    }
}
