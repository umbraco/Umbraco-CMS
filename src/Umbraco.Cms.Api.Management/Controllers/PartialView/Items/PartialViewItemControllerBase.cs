using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Items;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
public class PartialViewItemControllerBase : ManagementApiControllerBase
{
}
