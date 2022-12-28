using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Filters;

public class RequireRuntimeLevelAttribute : ActionFilterAttribute
{
    private readonly RuntimeLevel _requiredRuntimeLevel;

    public RequireRuntimeLevelAttribute(RuntimeLevel requiredRuntimeLevel) =>
        _requiredRuntimeLevel = requiredRuntimeLevel;

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
