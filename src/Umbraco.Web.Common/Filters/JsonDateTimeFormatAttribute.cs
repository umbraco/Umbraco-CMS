using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
/// Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
/// </summary>
public sealed class JsonDateTimeFormatAttribute : TypeFilterAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="JsonDateTimeFormatAttribute" /> class.
    /// </summary>
    public JsonDateTimeFormatAttribute()
        : base(typeof(JsonDateTimeFormatFilter)) =>
        Order = 2; // must be higher than AngularJsonOnlyConfigurationAttribute.Order

    private class JsonDateTimeFormatFilter : IResultFilter
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";
        private readonly MvcOptions _options;

        public JsonDateTimeFormatFilter(ArrayPool<char> arrayPool, IOptionsSnapshot<MvcOptions> options)
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
                serializerSettings.Converters.Add(
                    new IsoDateTimeConverter { DateTimeFormat = _format });
                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(
                    new AngularJsonMediaTypeFormatter(serializerSettings, _arrayPool, _options));
            }
        }
    }
}
