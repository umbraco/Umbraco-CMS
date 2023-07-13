using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MediaType)]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMediaTypes)]
public class MediaTypeItemControllerBase : ManagementApiControllerBase
{
}
