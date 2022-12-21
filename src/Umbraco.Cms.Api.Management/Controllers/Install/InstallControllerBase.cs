using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

[ApiController]
[VersionedApiBackOfficeRoute("install")]
[ApiExplorerSettings(GroupName = "Install")]
[RequireRuntimeLevel(RuntimeLevel.Install)]
public abstract class InstallControllerBase : ManagementApiControllerBase
{
}
