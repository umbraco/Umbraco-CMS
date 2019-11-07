using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Umbraco.Core.Serialization
{
    public class JsonNetSerializer : IJsonSerializer
    {
        public string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public T Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }
    }
}
