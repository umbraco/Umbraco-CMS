using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
public class ScriptItemControllerBase : ManagementApiControllerBase
{
}
