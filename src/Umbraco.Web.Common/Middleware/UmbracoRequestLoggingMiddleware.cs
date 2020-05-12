using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog.Context;
using Umbraco.Core;
using Umbraco.Core.Logging.Serilog.Enrichers;

namespace Umbraco.Web.Common.Middleware
{
    public class UmbracoRequestLoggingMiddleware
    {
        readonly RequestDelegate _next;
        private readonly HttpSessionIdEnricher _sessionIdEnricher;
        private readonly HttpRequestNumberEnricher _requestNumberEnricher;
        private readonly HttpRequestIdEnricher _requestIdEnricher;        

        public UmbracoRequestLoggingMiddleware(RequestDelegate next,
            HttpSessionIdEnricher sessionIdEnricher,
            HttpRequestNumberEnricher requestNumberEnricher,
            HttpRequestIdEnricher requestIdEnricher)
        {
            _next = next;
            _sessionIdEnricher = sessionIdEnricher;
            _requestNumberEnricher = requestNumberEnricher;
            _requestIdEnricher = requestIdEnricher;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // do not process if client-side request
            if (new Uri(httpContext.Request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsClientSideRequest())
            {
                await _next(httpContext);
                return;
            }

            // TODO: Need to decide if we want this stuff still, there's new request logging in serilog:
            // https://github.com/serilog/serilog-aspnetcore#request-logging which i think would suffice and replace all of this?

            using (LogContext.Push(_sessionIdEnricher))
            using (LogContext.Push(_requestNumberEnricher))
            using (LogContext.Push(_requestIdEnricher))
            {
                await _next(httpContext);
            }
        }
    }
}
