using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiController]
[VersionedApiBackOfficeRoute("package")]
[ApiExplorerSettings(GroupName = "Package")]
[ApiVersion("1.0")]
public abstract class PackageControllerBase : ManagementApiControllerBase
{
}
