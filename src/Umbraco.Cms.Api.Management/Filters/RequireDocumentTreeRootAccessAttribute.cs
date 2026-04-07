using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Filters;

/// <summary>
/// Specifies an attribute that enforces a requirement for the user to have access to the root of the document tree.
/// Apply this attribute to controllers or actions to restrict access accordingly.
/// </summary>
public class RequireDocumentTreeRootAccessAttribute : RequireTreeRootAccessAttribute
{
    protected override int[] GetUserStartNodeIds(IUser user, ActionExecutingContext context)
    {
        AppCaches appCaches = context.HttpContext.RequestServices.GetRequiredService<AppCaches>();
        IEntityService entityService = context.HttpContext.RequestServices.GetRequiredService<IEntityService>();

        return user.CalculateContentStartNodeIds(entityService, appCaches) ?? Array.Empty<int>();
    }
}
