using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.ManagementApi.Filters;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.Upgrade;

// TODO: This needs to be an authorized controller.

[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{

}
