using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[VersionedApiBackOfficeRoute(Constants.UdiEntityType.DataType)]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentsOrMediaOrMembersOrContentTypes)]
public abstract class DataTypeControllerBase : ManagementApiControllerBase
{
    protected IActionResult DataTypeOperationStatusResult(DataTypeOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            DataTypeOperationStatus.InvalidConfiguration => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid data type configuration")
                .WithDetail("The supplied data type configuration was not valid. Please see the log for more details.")
                .Build()),
            DataTypeOperationStatus.NotFound => DataTypeNotFound(problemDetailsBuilder),
            DataTypeOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid data type name")
                .WithDetail("The data type name must be non-empty and no longer than 255 characters.")
                .Build()),
            DataTypeOperationStatus.ParentNotContainer => BadRequest(problemDetailsBuilder
                .WithTitle("parent id is not a container")
                .WithDetail("The parent id does not represent a container.")
                .Build()),
            DataTypeOperationStatus.DuplicateKey => BadRequest(problemDetailsBuilder
                .WithTitle("The id is already used")
                .WithDetail("The data type id must be unique.")
                .Build()),
            DataTypeOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the data type operation.")
                .Build()),
            DataTypeOperationStatus.PropertyEditorNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The targeted property editor was not found.")
                .Build()),
            DataTypeOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The targeted parent for the data type operation was not found.")
                .Build()),
            DataTypeOperationStatus.NonDeletable => BadRequest(problemDetailsBuilder
                .WithTitle("The data type is non-deletable")
                .WithDetail("The specified data type is required by the system and cannot be deleted.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown data type operation status.")
                .Build()),
        });

    protected IActionResult DataTypeNotFound() => OperationStatusResult(DataTypeOperationStatus.NotFound, DataTypeNotFound);

    private IActionResult DataTypeNotFound(ProblemDetailsBuilder problemDetailsBuilder)
        => NotFound(problemDetailsBuilder
            .WithTitle("The data type could not be found")
            .Build());
}
