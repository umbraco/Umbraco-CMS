using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DataType)]
[ApiExplorerSettings(GroupName = "Data Type")]
[ApiVersion("1.0")]
public abstract class DataTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult DataTypeOperationStatusResult(DataTypeOperationStatus status) =>
        status switch
        {
            DataTypeOperationStatus.InvalidConfiguration => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid data type configuration")
                .WithDetail("The supplied data type configuration was not valid. Please see the log for more details.")
                .Build()),
            DataTypeOperationStatus.NotFound => NotFound("The data type could not be found"),
            DataTypeOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid data type name")
                .WithDetail("The data type name must be non-empty and no longer than 255 characters.")
                .Build()),
            DataTypeOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the data type operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown data type operation status")
        };
}
