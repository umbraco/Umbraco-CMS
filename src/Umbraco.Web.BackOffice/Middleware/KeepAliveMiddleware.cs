using Microsoft.AspNetCore.Http;

namespace Umbraco.Cms.Web.BackOffice.Middleware;

/// <summary>
///     Used for the Umbraco keep alive service. This is terminating middleware.
/// </summary>
public class KeepAliveMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("I'm alive");
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
