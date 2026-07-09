using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Delivery.Routing;

/// <summary>
/// A routing attribute that ensures consistent Delivery API endpoint paths.
/// </summary>
public sealed class VersionedDeliveryApiRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedDeliveryApiRouteAttribute(string template)
        : base($"delivery/api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
