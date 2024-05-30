using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Item;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Item}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
public class PartialViewItemControllerBase : ManagementApiControllerBase
{
}
