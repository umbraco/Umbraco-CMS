using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult PartialViewOperationStatusResult(PartialViewOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder =>
            status switch
            {
                PartialViewOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                    .WithTitle("Partial view already exists")
                    .WithDetail("A partial view with the same path already exists")
                    .Build()),
                PartialViewOperationStatus.InvalidFileExtension => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid file extension")
                    .WithDetail("The file extension is not valid for a partial view.")
                    .Build()),
                PartialViewOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                    .WithTitle("Parent not found")
                    .WithDetail("The parent folder was not found.")
                    .Build()),
                PartialViewOperationStatus.PathTooLong => BadRequest(problemDetailsBuilder
                    .WithTitle("Path too long")
                    .WithDetail("The file path is too long.")
                    .Build()),
                PartialViewOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid name")
                    .WithDetail("The partial view name is invalid.")
                    .Build()),
                PartialViewOperationStatus.NotFound => PartialViewNotFound(),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown partial view operation status.")
                    .Build()),
            });

    protected IActionResult PartialViewNotFound() => OperationStatusResult(
        PartialViewOperationStatus.NotFound,
        problemDetailsBuilder => NotFound(problemDetailsBuilder
            .WithTitle("Partial view not found")
            .WithDetail("The partial view was not found.")
            .Build()));

}
