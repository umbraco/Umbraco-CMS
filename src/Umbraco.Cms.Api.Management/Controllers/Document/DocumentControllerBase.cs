using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Content;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.Document)]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Document))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentControllerBase : ContentControllerBase
{
    protected IActionResult ContentAuthorizationStatusResult(ContentAuthorizationStatus status) =>
        status switch
        {
            ContentAuthorizationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("The content item could not be found")
                .Build()),
            ContentAuthorizationStatus.UnauthorizedMissingBinAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized recycle bin access")
                .WithDetail("The performing user does not have access to the content recycle bin item.")
                .Build()),
            ContentAuthorizationStatus.UnauthorizedMissingDescendantAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized bin access")
                .WithDetail("The performing user does not have access to all descendant items.")
                .Build()),
            ContentAuthorizationStatus.UnauthorizedMissingPathAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized")
                .WithDetail("The performing user does not have access to all specified content items.")
                .Build()),
            ContentAuthorizationStatus.UnauthorizedMissingRootAccess => Unauthorized(new ProblemDetailsBuilder()
                .WithTitle("Unauthorized root access")
                .WithDetail("The performing user does not have access to the content root item.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown content authorization status.")
                .Build()),
        };

    protected IActionResult DocumentNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The requested Document could not be found")
        .Build());
}
