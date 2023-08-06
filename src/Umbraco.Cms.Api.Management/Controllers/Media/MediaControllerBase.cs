using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Media)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMedia)]
public class MediaControllerBase : ContentControllerBase
{
    protected IActionResult MediaNotFound() => NotFound("The requested Media could not be found");
}
