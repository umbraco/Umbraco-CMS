using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Filters;

public class OutgoingNoHyphenGuidFormatAttribute : TypeFilterAttribute
{
    public OutgoingNoHyphenGuidFormatAttribute()
        : base(typeof(OutgoingNoHyphenGuidFormatFilter)) =>
        Order = 2; // must be higher than AngularJsonOnlyConfigurationAttribute.Order

    private class OutgoingNoHyphenGuidFormatFilter : IResultFilter
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly MvcOptions _options;

        public OutgoingNoHyphenGuidFormatFilter(ArrayPool<char> arrayPool, IOptionsSnapshot<MvcOptions> options)
        {
            _arrayPool = arrayPool;
            _options = options.Value;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.Converters.Add(new GuidNoHyphenConverter());

                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(
                    new AngularJsonMediaTypeFormatter(serializerSettings, _arrayPool, _options));
            }
        }

        /// <summary>
        ///     A custom converter for GUID's to format without hyphens
        /// </summary>
        private class GuidNoHyphenConverter : JsonConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Null:
                        return Guid.Empty;
                    case JsonToken.String:
                        Attempt<Guid> guidAttempt = reader.Value.TryConvertTo<Guid>();
                        if (guidAttempt.Success)
                        {
                            return guidAttempt.Result;
                        }

                        throw new FormatException("Could not convert " + reader.Value + " to a GUID");
                    default:
                        throw new ArgumentException("Invalid token type");
                }
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
                writer.WriteValue(Guid.Empty.Equals(value) ? Guid.Empty.ToString("N") : ((Guid?)value)?.ToString("N"));

            public override bool CanConvert(Type objectType) => typeof(Guid) == objectType;
        }
    }
}
