using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

[AllowAnonymous]
[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{

}
