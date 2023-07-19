using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Items;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewItemControllerBase : ManagementApiControllerBase
{
}
