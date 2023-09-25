using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Media)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Media))]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessMedia)]
public class MediaControllerBase : ContentControllerBase
{
    protected IActionResult MediaAuthorizationStatusResult(MediaAuthorizationStatus status) =>
        status switch
        {
            MediaAuthorizationStatus.UnauthorizedMissingBinAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized bin access")
                .WithDetail("The performing user does not have access to the media bin item.")
                .Build()),
            MediaAuthorizationStatus.UnauthorizedMissingPathAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized")
                .WithDetail("The performing user does not have access to all specified media items.")
                .Build()),
            MediaAuthorizationStatus.UnauthorizedMissingRootAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized root access")
                .WithDetail("The performing user does not have access to the media root item.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown media authorization status.")
                .Build()),
        };

    protected IActionResult MediaNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The requested Media could not be found")
        .Build());
}
