using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

/// <summary>
/// Serves as the base class for controllers handling server-related API management operations in Umbraco.
/// </summary>
[VersionedApiBackOfficeRoute("server")]
[ApiExplorerSettings(GroupName = "Server")]
public abstract class ServerControllerBase : ManagementApiControllerBase
{
}
