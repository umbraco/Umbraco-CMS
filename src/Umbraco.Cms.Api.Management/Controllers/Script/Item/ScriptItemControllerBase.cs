using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessScripts)]
public class ScriptItemControllerBase : ManagementApiControllerBase
{
}
