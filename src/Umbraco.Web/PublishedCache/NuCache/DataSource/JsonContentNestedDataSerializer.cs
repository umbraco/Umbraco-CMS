using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    public class JsonContentNestedDataSerializer : IContentCacheDataSerializer
    {
        public ContentCacheDataModel Deserialize(int contentTypeId, string stringData, byte[] byteData)
        {
            if (byteData != null && stringData == null)
                throw new NotSupportedException($"{typeof(JsonContentNestedDataSerializer)} does not support byte[] serialization");

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

            return JsonConvert.DeserializeObject<ContentCacheDataModel>(stringData, settings);
        }

        public ContentCacheDataSerializationResult Serialize(int contentTypeId, ContentCacheDataModel model)
        {
            // note that numeric values (which are Int32) are serialized without their
            // type (eg "value":1234) and JsonConvert by default deserializes them as Int64

            var json = JsonConvert.SerializeObject(model);
            return new ContentCacheDataSerializationResult(json, null);
        }
    }
}
