using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Applying this attribute to any controller will ensure that it only contains one json formatter compatible with the
///     angular json vulnerability prevention.
/// </summary>
public class AngularJsonOnlyConfigurationAttribute : TypeFilterAttribute
{
    public AngularJsonOnlyConfigurationAttribute()
        : base(typeof(AngularJsonOnlyConfigurationFilter)) =>
        Order = 1; // Must be low, to be overridden by other custom formatters, but higher then all framework stuff.

    private class AngularJsonOnlyConfigurationFilter : IResultFilter
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly MvcOptions _options;

        public AngularJsonOnlyConfigurationFilter(ArrayPool<char> arrayPool, IOptionsSnapshot<MvcOptions> options)
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
                    ContractResolver = new DefaultContractResolver(),
                    Converters = { new VersionConverter() },
                };

                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(
                    new AngularJsonMediaTypeFormatter(serializerSettings, _arrayPool, _options));
            }
        }
    }
}
