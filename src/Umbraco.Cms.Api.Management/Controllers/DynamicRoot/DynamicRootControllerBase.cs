using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.DynamicRoot;

[ApiController]
[VersionedApiBackOfficeRoute("dynamic-root")]
[ApiExplorerSettings(GroupName = "Dynamic Root")]
public abstract class DynamicRootControllerBase : ManagementApiControllerBase
{
}
