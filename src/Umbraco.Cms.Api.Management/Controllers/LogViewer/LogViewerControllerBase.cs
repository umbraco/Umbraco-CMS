using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.LogViewer;

[VersionedApiBackOfficeRoute("log-viewer")]
[ApiExplorerSettings(GroupName = "Log Viewer")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public abstract class LogViewerControllerBase : ManagementApiControllerBase
{
    protected IActionResult LogViewerOperationStatusResult(LogViewerOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder =>
            status switch
            {
                LogViewerOperationStatus.NotFoundLogSearch => NotFound(problemDetailsBuilder
                    .WithTitle("The log search could not be found")
                    .Build()),
                LogViewerOperationStatus.DuplicateLogSearch => BadRequest(problemDetailsBuilder
                    .WithTitle("Duplicate log search name")
                    .WithDetail("Another log search already exists with the attempted name.")
                    .Build()),
                LogViewerOperationStatus.CancelledByLogsSizeValidation => BadRequest(problemDetailsBuilder
                    .WithTitle("Cancelled due to log file size")
                    .WithDetail("The log file size for the requested date range prevented the operation.")
                    .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown log viewer operation status.")
                    .Build()),
            });
}
