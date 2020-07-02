using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal class JsonContentNestedDataSerializer : IContentNestedDataSerializer
    {
        public ContentNestedData Deserialize(string data)
        {
            // by default JsonConvert will deserialize our numeric values as Int64
            // which is bad, because they were Int32 in the database - take care

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ForceInt32Converter() }
            };

            return JsonConvert.DeserializeObject<ContentNestedData>(data, settings);
        }

        public string Serialize(ContentNestedData nestedData)
        {
            return JsonConvert.SerializeObject(nestedData);
        }
    }
}
