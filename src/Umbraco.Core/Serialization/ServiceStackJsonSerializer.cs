using System;
using System.IO;
using ServiceStack.Text;

namespace Umbraco.Core.Serialization
{
    public class ServiceStackJsonSerializer : ISerializer
    {
        public ServiceStackJsonSerializer()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig.ExcludeTypeInfo = false;
            JsConfig.IncludeNullValues = true;
            JsConfig.ThrowOnDeserializationError = true;
        }

        public object FromStream(Stream input, Type outputType)
        {
            return JsonSerializer.DeserializeFromStream(outputType, input);
        }

        public IStreamedResult ToStream(object input)
        {
            var ms = new MemoryStream();
            JsonSerializer.SerializeToStream(input, ms);
            return new StreamedResult(ms, true);
        }
    }
}