using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
public class PartialViewControllerBase : ManagementApiControllerBase
{
    protected IActionResult PartialViewOperationStatusResult(PartialViewOperationStatus status) =>
        status switch
        {
            PartialViewOperationStatus.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown partial view operation status")
        };
}
