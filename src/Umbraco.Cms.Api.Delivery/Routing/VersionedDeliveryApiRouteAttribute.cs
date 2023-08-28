using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Delivery.Routing;

internal sealed class VersionedDeliveryApiRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedDeliveryApiRouteAttribute(string template)
        : base($"delivery/api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
