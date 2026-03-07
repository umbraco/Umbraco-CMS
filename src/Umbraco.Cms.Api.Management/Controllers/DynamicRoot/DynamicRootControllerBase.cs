using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

    /// <summary>
    /// Serves as the base class for dynamic root controllers in the Umbraco CMS Management API.
    /// </summary>
[VersionedApiBackOfficeRoute("dynamic-root")]
[ApiExplorerSettings(GroupName = "Dynamic Root")]
public abstract class DynamicRootControllerBase : ManagementApiControllerBase
{
}
