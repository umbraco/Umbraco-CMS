using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Web.BackOffice.Filters;

public class JsonCamelCaseFormatterAttribute : TypeFilterAttribute
{
    public JsonCamelCaseFormatterAttribute() : base(typeof(JsonCamelCaseFormatterFilter)) =>
        Order = 2; //must be higher than AngularJsonOnlyConfigurationAttribute.Order

    private class JsonCamelCaseFormatterFilter : IResultFilter
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly MvcOptions _options;

        public JsonCamelCaseFormatterFilter(ArrayPool<char> arrayPool, IOptionsSnapshot<MvcOptions> options)
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
                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(
                    new AngularJsonMediaTypeFormatter(serializerSettings, _arrayPool, _options));
            }
        }
    }
}
