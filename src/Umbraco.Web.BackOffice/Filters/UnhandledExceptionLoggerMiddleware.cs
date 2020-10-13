using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Filters
{
    public class UnhandledExceptionLoggerMiddleware : IMiddleware
    {
        private readonly ILogger<UnhandledExceptionLoggerMiddleware> _logger;

        public UnhandledExceptionLoggerMiddleware(ILogger<UnhandledExceptionLoggerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestUri = new Uri(context.Request.GetEncodedUrl(), UriKind.RelativeOrAbsolute);
            // If it's a client side request just call next and don't try to log anything
            if (requestUri.IsClientSideRequest())
            {
                await next(context);
            }
            else
            {
                // We call the next middleware, and catch any errors that occurs in the rest of the pipeline
                try
                {
                    await next(context);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unhandled controller exception occurred for request '{RequestUrl}'", requestUri.AbsoluteUri);
                    // throw the error again, just in case it gets handled (which is shouldn't)
                    throw;
                }
            }
        }
    }
}
