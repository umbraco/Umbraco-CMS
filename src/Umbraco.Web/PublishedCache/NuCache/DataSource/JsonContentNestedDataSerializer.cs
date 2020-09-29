using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    internal class JsonContentNestedDataSerializer : IContentNestedDataSerializer
    {
        public ContentNestedData Deserialize(int contentTypeId, string data)
        {
            // by default JsonConvert will deserialize our numeric values as Int64
            // which is bad, because they were Int32 in the database - take care

            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ForceInt32Converter() },

                // Explicitly specify date handling so that it's consistent and follows the same date handling as MessagePack
                DateParseHandling = DateParseHandling.DateTime,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatString = "o"                
            };

            return JsonConvert.DeserializeObject<ContentNestedData>(data, settings);
        }

        public string Serialize(int contentTypeId, ContentNestedData nestedData)
        {
            // note that numeric values (which are Int32) are serialized without their
            // type (eg "value":1234) and JsonConvert by default deserializes them as Int64

            return JsonConvert.SerializeObject(nestedData);
        }
    }
}
