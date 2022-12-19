using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[ApiController]
[VersionedApiBackOfficeRoute("package/created")]
[ApiExplorerSettings(GroupName = "Package")]
[ApiVersion("1.0")]
public class CreatedPackageControllerBase : PackageControllerBase
{
}
