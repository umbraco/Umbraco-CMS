using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Document}-version")]
[ApiExplorerSettings(GroupName = $"{nameof(Constants.UdiEntityType.Document)} Version")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
public abstract class DocumentVersionControllerBase : ManagementApiControllerBase
{
    protected IActionResult MapFailure(ContentVersionOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentVersionOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested version could not be found")
                .Build()),
            ContentVersionOperationStatus.ContentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The requested document could not be found")
                .Build()),
            ContentVersionOperationStatus.InvalidSkipTake => SkipTakeToPagingProblem(),
            ContentVersionOperationStatus.RollBackFailed => BadRequest(problemDetailsBuilder
                .WithTitle("Rollback failed")
                .WithDetail("An unspecified error occurred while rolling back the requested version. Please check the logs for additional information.")),
            ContentVersionOperationStatus.RollBackCanceled => BadRequest(problemDetailsBuilder
                .WithTitle("Request cancelled by notification")
                .WithDetail("The request to roll back was cancelled by a notification handler.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown content version operation status.")
                .Build()),
        });
}
