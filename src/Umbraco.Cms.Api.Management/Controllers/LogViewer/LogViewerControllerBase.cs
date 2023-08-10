using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[ApiController]
[VersionedApiBackOfficeRoute("log-viewer")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessSettings)]
public abstract class LogViewerControllerBase : ManagementApiControllerBase
{
    protected IActionResult LogViewerOperationStatusResult(LogViewerOperationStatus status) =>
        status switch
        {
            LogViewerOperationStatus.NotFoundLogSearch => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The log search could not be found")
                    .Build()),
            LogViewerOperationStatus.DuplicateLogSearch => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Duplicate log search name")
                .WithDetail("Another log search already exists with the attempted name.")
                .Build()),
            LogViewerOperationStatus.CancelledByLogsSizeValidation => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled due to log file size")
                .WithDetail("The log file size for the requested date range prevented the operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown log viewer operation status.")
                .Build()),
        };
}
