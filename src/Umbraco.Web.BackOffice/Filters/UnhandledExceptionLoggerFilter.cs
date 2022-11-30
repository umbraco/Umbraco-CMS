using Microsoft.AspNetCore.Builder;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Applies the UnhandledExceptionLoggerMiddleware to a specific controller
///     when used with this attribute [MiddlewareFilter(typeof(UnhandledExceptionLoggerFilter))]
///     The middleware will run in the filter pipeline, at the same stage as resource filters
/// </summary>
public class UnhandledExceptionLoggerFilter
{
    public void Configure(IApplicationBuilder applicationBuilder) =>
        applicationBuilder.UseMiddleware<UnhandledExceptionLoggerMiddleware>();
}
