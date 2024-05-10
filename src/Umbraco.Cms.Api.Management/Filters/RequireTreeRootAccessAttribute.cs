using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Filters;

public abstract class RequireTreeRootAccessAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor = context.HttpContext.RequestServices.GetRequiredService<IBackOfficeSecurityAccessor>();
        IUser? user = backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;

        var startNodeIds = user != null ? GetUserStartNodeIds(user, context) : Array.Empty<int>();
        if (startNodeIds.Contains(Constants.System.Root))
        {
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Forbidden",
            Detail = "The current backoffice user should have access to the tree root",
            Status = StatusCodes.Status403Forbidden,
            Type = "Error",
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
    }

    protected abstract int[] GetUserStartNodeIds(IUser user, ActionExecutingContext context);
}
