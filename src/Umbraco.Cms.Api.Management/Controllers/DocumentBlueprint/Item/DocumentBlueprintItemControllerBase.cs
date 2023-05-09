using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Item;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
public class DocumentBlueprintItemControllerBase : ManagementApiControllerBase
{
}
