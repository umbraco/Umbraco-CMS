using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiController]
[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DataType)]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public abstract class DataTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult DataTypeOperationStatusResult(DataTypeOperationStatus status) =>
        status switch
        {
            DataTypeOperationStatus.InvalidConfiguration => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid data type configuration")
                .WithDetail("The supplied data type configuration was not valid. Please see the log for more details.")
                .Build()),
            DataTypeOperationStatus.NotFound => DataTypeNotFound(),
            DataTypeOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid data type name")
                .WithDetail("The data type name must be non-empty and no longer than 255 characters.")
                .Build()),
            DataTypeOperationStatus.ParentNotContainer => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("parent id is not a container")
                .WithDetail("The parent id does not represent a container.")
                .Build()),
            DataTypeOperationStatus.DuplicateKey => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The id is already used")
                .WithDetail("The data type id must be unique.")
                .Build()),
            DataTypeOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the data type operation.")
                .Build()),
            DataTypeOperationStatus.PropertyEditorNotFound => NotFound(
                new ProblemDetailsBuilder()
                    .WithTitle("The targeted property editor was not found.")
                    .Build()),
            DataTypeOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The targeted parent for the data type operation was not found.")
                    .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown data type operation status.")
                .Build()),
        };

    protected IActionResult DataTypeNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The data type could not be found")
        .Build());
}
