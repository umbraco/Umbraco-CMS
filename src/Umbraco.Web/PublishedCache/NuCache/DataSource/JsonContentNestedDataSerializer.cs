using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{

    public class JsonContentNestedDataSerializer : IContentCacheDataSerializer
    {
        // by default JsonConvert will deserialize our numeric values as Int64
        // which is bad, because they were Int32 in the database - take care
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new ForceInt32Converter() },

            // Explicitly specify date handling so that it's consistent and follows the same date handling as MessagePack
            DateParseHandling = DateParseHandling.DateTime,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatString = "o"
        };
        private readonly JsonNameTable _propertyNameTable = new DefaultJsonNameTable();
        public ContentCacheDataModel Deserialize(IReadOnlyContentBase content, string stringData, byte[] byteData, bool published)
        {
            if (stringData == null && byteData != null)
                throw new NotSupportedException($"{typeof(JsonContentNestedDataSerializer)} does not support byte[] serialization");

            JsonSerializer serializer = JsonSerializer.Create(_jsonSerializerSettings);
            using (JsonTextReader reader = new JsonTextReader(new StringReader(stringData)))
            {
                // reader will get buffer from array pool
                reader.ArrayPool = JsonArrayPool.Instance;
                reader.PropertyNameTable = _propertyNameTable;
                return serializer.Deserialize<ContentCacheDataModel>(reader);
            }
        }

        public ContentCacheDataSerializationResult Serialize(IReadOnlyContentBase content, ContentCacheDataModel model, bool published)
        {
            // note that numeric values (which are Int32) are serialized without their
            // type (eg "value":1234) and JsonConvert by default deserializes them as Int64

            var json = JsonConvert.SerializeObject(model);
            return new ContentCacheDataSerializationResult(json, null);
        }
    }
    public class JsonArrayPool : IArrayPool<char>
    {
        public static readonly JsonArrayPool Instance = new JsonArrayPool();

        public char[] Rent(int minimumLength)
        {
            // get char array from System.Buffers shared pool
            return ArrayPool<char>.Shared.Rent(minimumLength);
        }

        public void Return(char[] array)
        {
            // return char array to System.Buffers shared pool
            ArrayPool<char>.Shared.Return(array);
        }
    }
    public class AutomaticJsonNameTable : DefaultJsonNameTable
    {
        int nAutoAdded = 0;
        int maxToAutoAdd;

        public AutomaticJsonNameTable(int maxToAdd)
        {
            this.maxToAutoAdd = maxToAdd;
        }

        public override string Get(char[] key, int start, int length)
        {
            var s = base.Get(key, start, length);

            if (s == null && nAutoAdded < maxToAutoAdd)
            {
                s = new string(key, start, length);
                Add(s);
                nAutoAdded++;
            }

            return s;
        }
    }
}
