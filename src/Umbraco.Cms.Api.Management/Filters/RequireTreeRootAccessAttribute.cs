using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Api.Management.Filters;

    /// <summary>
    /// An attribute that enforces a requirement for the current user to have access permissions to the root node of a specified tree structure within the Umbraco backoffice.
    /// Apply this attribute to controller actions or controllers to restrict access to users with root-level permissions for the relevant tree.
    /// </summary>
public abstract class RequireTreeRootAccessAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Called before the action executes to ensure that the current backoffice user has access to the tree root.
    /// If the user does not have access, sets the result to a 403 Forbidden response and prevents the action from executing.
    /// </summary>
    /// <param name="context">The context for the action being executed.</param>
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
