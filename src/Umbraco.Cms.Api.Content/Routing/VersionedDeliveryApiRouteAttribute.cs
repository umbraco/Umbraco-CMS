using Umbraco.Cms.Api.Common.Routing;

namespace Umbraco.Cms.Api.Content.Routing;

public class VersionedDeliveryApiRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedDeliveryApiRouteAttribute(string template)
        : base($"delivery/api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
