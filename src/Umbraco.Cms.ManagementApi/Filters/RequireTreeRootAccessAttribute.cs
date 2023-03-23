using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.ManagementApi.Filters;

public abstract class RequireTreeRootAccessAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor = context.HttpContext.RequestServices.GetRequiredService<IBackOfficeSecurityAccessor>();
        IUser? user = backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        var startNodeIds = user != null ? GetUserStartNodeIds(user, context) : Array.Empty<int>();

        // TODO: remove this once we have backoffice auth in place
        startNodeIds = new[] { Constants.System.Root };

        if (startNodeIds.Contains(Constants.System.Root))
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Unauthorized user",
            Detail = "The current backoffice user should have access to the tree root",
            Status = StatusCodes.Status401Unauthorized,
            Type = "Error",
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status401Unauthorized };
    }

    protected abstract int[] GetUserStartNodeIds(IUser user, ActionExecutingContext context);
}
