using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Tour;

[VersionedApiBackOfficeRoute("tour")]
[ApiExplorerSettings(GroupName = "Tour")]
public class TourControllerBase : ManagementApiControllerBase
{
    protected IActionResult TourOperationStatusResult(TourOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            TourOperationStatus.Success => Ok(),
            TourOperationStatus.UserNotFound => NotFound(problemDetailsBuilder
                .WithTitle("User not found")
                .WithDetail("Was not able to find currently logged in user")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown tour operation status.")
                .Build()),
        });
}
