using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
public class ScriptItemControllerBase : ManagementApiControllerBase
{
}
