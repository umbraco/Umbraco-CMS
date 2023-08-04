using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.MediaType)]
[ApiExplorerSettings(GroupName = "Media Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessMediaTypes)]
public abstract class MediaTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult MediaTypeNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The media type could not be found")
        .Build());


}
