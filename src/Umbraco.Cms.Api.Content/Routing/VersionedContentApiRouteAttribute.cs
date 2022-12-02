using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Content.Routing;

public class VersionedContentApiRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedContentApiRouteAttribute(string template)
        : base($"content/api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}

// TODO: refactor - move BackOfficeRouteAttribute from Management API to Common and delete this one
public class BackOfficeRouteAttribute : RouteAttribute
{
    // All this does is append [umbracoBackoffice]/ to the route,
    // this is then replaced with whatever is configures as UmbracoPath by the UmbracoBackofficeToken convention
    public BackOfficeRouteAttribute(string template)
        : base($"[{Constants.Web.AttributeRouting.BackOfficeToken}]/" + template.TrimStart('/'))
    {
    }
}
