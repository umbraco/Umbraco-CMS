using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Filters;

public class RequireDocumentTreeRootAccessAttribute : RequireTreeRootAccessAttribute
{
    protected override int[] GetUserStartNodeIds(IUser user, ActionExecutingContext context)
    {
        AppCaches appCaches = context.HttpContext.RequestServices.GetRequiredService<AppCaches>();
        IEntityService entityService = context.HttpContext.RequestServices.GetRequiredService<IEntityService>();

        return user.CalculateContentStartNodeIds(entityService, appCaches) ?? Array.Empty<int>();
    }
}
