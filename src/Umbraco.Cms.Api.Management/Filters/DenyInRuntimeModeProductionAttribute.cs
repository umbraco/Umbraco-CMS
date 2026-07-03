using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Filters;

/// <summary>
/// Short-circuits a request with a 400 (Bad Request) response when the application is running in
/// <see cref="RuntimeMode.Production" />. Use this to make schema-editing endpoints read-only in production,
/// where the deployment artifact - not the live database - is the source of truth for schema.
/// </summary>
/// <remarks>
/// This only gates the (interactive) Management API surface. It deliberately does not touch the underlying
/// core services, so programmatic writes from deployments, package installs and upgrade migrations continue
/// to work while the site runs in production runtime mode.
/// </remarks>
public sealed class DenyInRuntimeModeProductionAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Called before the action executes and short-circuits the request when the runtime mode is
    /// <see cref="RuntimeMode.Production" />.
    /// </summary>
    /// <param name="context">The context for the executing action.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IOptionsMonitor<RuntimeSettings> runtimeSettings =
            context.HttpContext.RequestServices.GetRequiredService<IOptionsMonitor<RuntimeSettings>>();

        if (runtimeSettings.CurrentValue.Mode != RuntimeMode.Production)
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Not allowed in production mode",
            Detail = "This operation is not allowed while the runtime mode is set to Production.",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status400BadRequest };
    }
}
