using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging.Serilog.Enrichers;
using Umbraco.Net;

namespace Umbraco.Web.Common.Middleware
{
    public class UmbracoRequestLoggingMiddleware
    {
        readonly RequestDelegate _next;
        private readonly ISessionIdResolver _sessionIdResolver;
        private readonly IRequestCache _requestCache;

        public UmbracoRequestLoggingMiddleware(RequestDelegate next, ISessionIdResolver sessionIdResolver, IRequestCache requestCache)
        {
            _next = next;
            _sessionIdResolver = sessionIdResolver;
            _requestCache = requestCache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // TODO: Need to decide if we want this stuff still, there's new request logging in serilog:
            // https://github.com/serilog/serilog-aspnetcore#request-logging which i think would suffice and replace all of this?

            using (LogContext.Push(new HttpSessionIdEnricher(_sessionIdResolver)))
            using (LogContext.Push(new HttpRequestNumberEnricher(_requestCache)))
            using (LogContext.Push(new HttpRequestIdEnricher(_requestCache)))
            {
                await _next(httpContext);
            }
        }
    }
}
