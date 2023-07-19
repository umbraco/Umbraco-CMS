using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Upgrade;

[ApiController]
[RequireRuntimeLevel(RuntimeLevel.Upgrade)]
[VersionedApiBackOfficeRoute("upgrade")]
[ApiExplorerSettings(GroupName = "Upgrade")]
[Authorize(Policy = "New" + AuthorizationPolicies.RequireAdminAccess)]
public abstract class UpgradeControllerBase : ManagementApiControllerBase
{

}
