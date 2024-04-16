using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Management.Routing;

public class VersionedApiBackOfficeRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedApiBackOfficeRouteAttribute(string template)
        : base($"{Constants.Web.ManagementApiPath}v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
