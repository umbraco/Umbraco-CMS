using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Filters;

/// <summary>
/// An attribute that enforces a requirement for the user to have access to the root of the media tree in order to execute the decorated action or controller.
/// </summary>
public class RequireMediaTreeRootAccessAttribute : RequireTreeRootAccessAttribute
{
    protected override int[] GetUserStartNodeIds(IUser user, ActionExecutingContext context)
    {
        AppCaches appCaches = context.HttpContext.RequestServices.GetRequiredService<AppCaches>();
        IEntityService entityService = context.HttpContext.RequestServices.GetRequiredService<IEntityService>();

        return user.CalculateMediaStartNodeIds(entityService, appCaches) ?? Array.Empty<int>();
    }
}
