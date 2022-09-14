using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.ManagementApi.Filters;

public class SystemTextJsonConfigurationAttribute : TypeFilterAttribute
{
    public SystemTextJsonConfigurationAttribute() : base(typeof(SystemTextJsonConfigurationFilter)) =>
        Order = 1; // Must be low, to be overridden by other custom formatters, but higher then all framework stuff.

    private class SystemTextJsonConfigurationFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                var serializerOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                objectResult.Formatters.Clear();
                objectResult.Formatters.Add(new EnumOutputFormatter(serializerOptions));
            }
        }
    }
}


