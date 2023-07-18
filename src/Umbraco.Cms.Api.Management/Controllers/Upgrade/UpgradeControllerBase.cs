using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

// TODO: This needs to be an authorized (at admin level) controller.

[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{

}
