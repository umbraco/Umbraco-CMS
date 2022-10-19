using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;

namespace Umbraco.New.Cms.Web.Common.Routing;

/// <summary>
/// Routes a controller within the backoffice area, I.E /umbraco
/// </summary>
public class BackOfficeRouteAttribute : RouteAttribute
{
    // All this does is append [umbracoBackoffice]/ to the route,
    // this is then replaced with whatever is configures as UmbracoPath by the UmbracoBackofficeToken convention
    public BackOfficeRouteAttribute(string template)
        : base($"[{Constants.Web.AttributeRouting.BackOfficeToken}]/" + template.TrimStart('/'))
    {
    }
}
