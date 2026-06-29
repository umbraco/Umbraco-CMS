using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Api.Management.Routing;

/// <summary>
/// Specifies an attribute used to define versioned API routes for back office endpoints in Umbraco.
/// Apply this attribute to controllers to indicate their route and API versioning in the back office context.
/// </summary>
public class VersionedApiBackOfficeRouteAttribute : BackOfficeRouteAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedApiBackOfficeRouteAttribute"/> class using the specified route template.
    /// </summary>
    /// <param name="template">The route template for the versioned API back office route.</param>
    public VersionedApiBackOfficeRouteAttribute(string template)
        : base($"{Constants.Web.ManagementApiPath}v{{version:apiVersion}}/{template.TrimStart('/')}")
    {
    }
}
