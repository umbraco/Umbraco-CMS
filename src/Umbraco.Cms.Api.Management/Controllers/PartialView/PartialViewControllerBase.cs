using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessPartialViews)]
public class PartialViewControllerBase : ManagementApiControllerBase
{
    protected IActionResult PartialViewOperationStatusResult(PartialViewOperationStatus status) =>
        status switch
        {
            PartialViewOperationStatus.Success => Ok(),
            PartialViewOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Partial view already exists")
                .WithDetail("A partial view with the same path already exists")
                .Build()),
            PartialViewOperationStatus.InvalidFileExtension => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file extension")
                .WithDetail("The file extension is not valid for a partial view.")
                .Build()),
            PartialViewOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            PartialViewOperationStatus.PathTooLong => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Path too long")
                .WithDetail("The file path is too long.")
                .Build()),
            PartialViewOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The partial view name is invalid.")
                .Build()),
            PartialViewOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Partial view not found")
                .WithDetail("The partial view was not found.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown partial view operation status")
        };
}
