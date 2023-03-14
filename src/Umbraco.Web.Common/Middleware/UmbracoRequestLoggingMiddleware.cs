using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Umbraco.Cms.Core.Logging.Serilog.Enrichers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware;

/// <summary>
///     Adds request based serilog enrichers to the LogContext for each request
/// </summary>
public class UmbracoRequestLoggingMiddleware : IMiddleware
{
    private readonly HttpRequestIdEnricher _requestIdEnricher;
    private readonly HttpRequestNumberEnricher _requestNumberEnricher;
    private readonly HttpSessionIdEnricher _sessionIdEnricher;

    public UmbracoRequestLoggingMiddleware(
        HttpSessionIdEnricher sessionIdEnricher,
        HttpRequestNumberEnricher requestNumberEnricher,
        HttpRequestIdEnricher requestIdEnricher)
    {
        _sessionIdEnricher = sessionIdEnricher;
        _requestNumberEnricher = requestNumberEnricher;
        _requestIdEnricher = requestIdEnricher;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // do not process if client-side request
        if (context.Request.IsClientSideRequest())
        {
            await next(context);
            return;
        }

        // TODO: Need to decide if we want this stuff still, there's new request logging in serilog:
        // https://github.com/serilog/serilog-aspnetcore#request-logging which i think would suffice and replace all of this?
        using (LogContext.Push(_sessionIdEnricher))
        using (LogContext.Push(_requestNumberEnricher))
        using (LogContext.Push(_requestIdEnricher))
        {
            await next(context);
        }
    }
}
