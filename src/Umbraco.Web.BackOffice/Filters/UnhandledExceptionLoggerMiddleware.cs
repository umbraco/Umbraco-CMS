using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Logs any unhandled exception.
/// </summary>
public class UnhandledExceptionLoggerMiddleware : IMiddleware
{
    private readonly ILogger<UnhandledExceptionLoggerMiddleware> _logger;

    public UnhandledExceptionLoggerMiddleware(ILogger<UnhandledExceptionLoggerMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // If it's a client side request just call next and don't try to log anything
        if (context.Request.IsClientSideRequest())
        {
            await next(context);
        }
        else
        {
            // Call the next middleware, and catch any errors that occurs in the rest of the pipeline
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled controller exception occurred for request '{RequestUrl}'",
                    context.Request.GetEncodedPathAndQuery());
                // Throw the error again, just in case it gets handled
                throw;
            }
        }
    }
}
