using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Middleware;

/// <summary>
///     Logs any unhandled exception.
/// </summary>
public class UnhandledExceptionLoggerMiddleware : IMiddleware
{
    private readonly ILogger<UnhandledExceptionLoggerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnhandledExceptionLoggerMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to log unhandled exceptions.</param>
    public UnhandledExceptionLoggerMiddleware(ILogger<UnhandledExceptionLoggerMiddleware> logger) => _logger = logger;

    /// <summary>
    /// Invokes the middleware to log any unhandled exceptions that occur during the request pipeline, except for client-side requests which are passed through without logging.
    /// </summary>
    /// <param name="context">The current HTTP context for the request.</param>
    /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    /// <remarks>
    /// If an unhandled exception occurs (excluding client-side requests), it is logged and then rethrown to allow further handling upstream.
    /// </remarks>
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
                _logger.LogError(
                    e,
                    "Unhandled controller exception occurred for request '{RequestUrl}'",
                    context.Request.GetEncodedPathAndQuery());
                // Throw the error again, just in case it gets handled
                throw;
            }
        }
    }
}
