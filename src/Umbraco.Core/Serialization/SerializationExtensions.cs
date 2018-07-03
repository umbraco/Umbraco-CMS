using System;
using System.IO;
using System.Text;

namespace Umbraco.Core.Serialization
{
    public static class SerializationExtensions
    {
        public static T FromJson<T>(this AbstractSerializationService service, string json, string intent = null)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            return (T)service.FromJson(json, typeof(T), intent);
        }

        public static T FromJson<T>(this ISerializer serializer, string json, string intent = null)
        {
            if (string.IsNullOrWhiteSpace(json)) return default(T);
            return (T)serializer.FromJson(json, typeof(T));
        }

        public static object FromJson(this ISerializer serializer, string json, Type outputType)
        {
            if (string.IsNullOrWhiteSpace(json)) return outputType.GetDefaultValue();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return serializer.FromStream(stream, outputType);
        }

        public static object FromJson(this AbstractSerializationService service, string json, Type outputType, string intent = null)
        {
            if (string.IsNullOrWhiteSpace(json)) return outputType.GetDefaultValue();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return service.FromStream(stream, outputType, intent);
        }

        public static string ToJson(this AbstractSerializationService service, object input, string intent = null)
        {
            return StreamResultExtensions.ToJsonString(service.ToStream(input, intent).ResultStream);
        }
    }
}
