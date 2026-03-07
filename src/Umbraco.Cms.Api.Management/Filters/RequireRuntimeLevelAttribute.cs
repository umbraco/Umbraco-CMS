using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Filters;

    /// <summary>
    /// Indicates that the decorated controller or action method requires the application to be at a specific runtime level in order to execute.
    /// Use this attribute to restrict access based on the current runtime state of the Umbraco application.
    /// </summary>
public class RequireRuntimeLevelAttribute : ActionFilterAttribute
{
    private readonly RuntimeLevel _requiredRuntimeLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireRuntimeLevelAttribute"/> class with the specified required runtime level.
    /// </summary>
    /// <param name="requiredRuntimeLevel">The runtime level required to allow access.</param>
    public RequireRuntimeLevelAttribute(RuntimeLevel requiredRuntimeLevel) =>
        _requiredRuntimeLevel = requiredRuntimeLevel;

    /// <summary>
    /// Called before the action executes to ensure that the current runtime level matches the required level.
    /// If the runtime level does not match the required value, the request is short-circuited and an error response is returned with status code 428 (Precondition Required).
    /// </summary>
    /// <param name="context">The context for the action executing, providing access to HTTP and action information.</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IRuntimeState runtimeState = context.HttpContext.RequestServices.GetRequiredService<IRuntimeState>();
        if (runtimeState.Level == _requiredRuntimeLevel)
        {
            return;
        }

        // We're not in the expected runtime level, so we need to short circuit
        var problemDetails = new ProblemDetails
        {
            Title = "Invalid runtime level",
            Detail = $"Runtime level {_requiredRuntimeLevel} is required",
            Status = StatusCodes.Status428PreconditionRequired,
            Type = "Error",
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status428PreconditionRequired };
    }
}
