namespace Umbraco.New.Cms.Web.Common.Routing;

public class VersionedApiBackOfficeRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedApiBackOfficeRouteAttribute(string template)
        : base($"management/api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
