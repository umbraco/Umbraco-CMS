using System.Buffers;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Web.Common.Formatters;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    /// Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the angular json vulnerability prevention.
    /// </summary>
    public class JsonDateTimeFormatAttribute : TypeFilterAttribute
    {
        public JsonDateTimeFormatAttribute() : base(typeof(JsonDateTimeFormatFilter))
        {
            Order = 2; //must be higher than AngularJsonOnlyConfigurationAttribute.Order
        }

        private class JsonDateTimeFormatFilter : IResultFilter
        {
            private readonly string _format = "yyyy-MM-dd HH:mm:ss";

            private readonly IOptions<MvcNewtonsoftJsonOptions> _mvcNewtonsoftJsonOptions;
            private readonly ArrayPool<char> _arrayPool;
            private readonly IOptions<MvcOptions> _options;

            public JsonDateTimeFormatFilter(IOptions<MvcNewtonsoftJsonOptions> mvcNewtonsoftJsonOptions, ArrayPool<char> arrayPool, IOptions<MvcOptions> options)
            {
                _mvcNewtonsoftJsonOptions = mvcNewtonsoftJsonOptions;
                _arrayPool = arrayPool;
                _options = options;
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
                        new IsoDateTimeConverter
                        {
                            DateTimeFormat = _format
                        });

                    objectResult.Formatters.Clear();
                    objectResult.Formatters.Add(new AngularJsonMediaTypeFormatter(serializerSettings, _arrayPool, _options.Value));
                }
            }
        }
    }


}
