using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Umbraco.Cms.Web.Common.Extensions;

public static class EndpointExtensions
{
    public static ControllerActionDescriptor? GetControllerActionDescriptor(this Endpoint endpoint)
    {
        return endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
    }

    public static RouteNameMetadata? GetRouteNameMetadata(this Endpoint endpoint)
    {
        return endpoint.Metadata.GetMetadata<RouteNameMetadata>();
    }

    public static string? GetRouteName(this Endpoint endpoint)
    {
        return endpoint.GetRouteNameMetadata()?.RouteName;
    }
}
