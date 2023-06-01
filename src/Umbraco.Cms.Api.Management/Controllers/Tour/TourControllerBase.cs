using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Tour;

[ApiController]
[VersionedApiBackOfficeRoute("tour")]
[ApiExplorerSettings(GroupName = "Tour")]
public class TourControllerBase : ManagementApiControllerBase
{
    protected IActionResult TourOperationStatusResult(TourOperationStatus status) =>
    status switch
    {
        TourOperationStatus.Success => Ok(),
        TourOperationStatus.UserNotFound => NotFound(new ProblemDetailsBuilder()
            .WithTitle("User not found")
            .WithDetail("Was not able to find currently logged in user")
            .Build()),
        _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown tour operation status.")
    };
}
