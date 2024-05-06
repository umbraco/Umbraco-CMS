using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Segment;

[VersionedApiBackOfficeRoute("segment")]
[ApiExplorerSettings(GroupName = "Segment")]
public abstract class SegmentControllerBase : ManagementApiControllerBase
{
    protected IActionResult MapFailure(SegmentOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown segment operation status.")
                .Build()),
        });
}
