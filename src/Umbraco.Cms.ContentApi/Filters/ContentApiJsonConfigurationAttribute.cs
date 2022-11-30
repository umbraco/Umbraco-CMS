using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.ContentApi.Filters;

public class ContentApiJsonConfigurationAttribute : TypeFilterAttribute
{
    public ContentApiJsonConfigurationAttribute()
        : base(typeof(SystemTextJsonConfigurationFilter)) =>
        // Must be low, to be overridden by other custom formatters, but higher then all framework stuff.
        Order = 1;

    private class SystemTextJsonConfigurationFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(new ContentApiJsonOutputFormatter());
            }
        }
    }
}
