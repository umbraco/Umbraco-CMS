using System;
using Newtonsoft.Json;
using Umbraco.Core;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// A custom converter for GUID's to format without hyphens
    /// </summary>
    internal class GuidNoHyphenConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return Guid.Empty;
                case JsonToken.String:
                    var guidAttempt = reader.Value.TryConvertTo<Guid>();
                    if (guidAttempt.Success)
                    {
                        return guidAttempt.Result;
                    }
                    throw new FormatException("Could not convert " + reader.Value + " to a GUID");
                default:
                    throw new ArgumentException("Invalid token type");
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Guid.Empty.Equals(value) ? Guid.Empty.ToString("N") : ((Guid)value).ToString("N"));
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Guid) == objectType;
        }
    }
}