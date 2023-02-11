namespace Umbraco.New.Cms.Web.Common.Routing;

public class VersionedApiBackOfficeRouteAttribute : BackOfficeRouteAttribute
{
    public VersionedApiBackOfficeRouteAttribute(string template)
        : base($"api/v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
