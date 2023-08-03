using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetControllerBase : ManagementApiControllerBase
{
    protected IActionResult StylesheetOperationStatusResult(StylesheetOperationStatus status) =>
        status switch
        {
            StylesheetOperationStatus.Success => Ok(),
            StylesheetOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Stylesheet already exists")
                .WithDetail("A stylesheet with the same path already exists")
                .Build()),
            StylesheetOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A stylesheet notification handler prevented the stylesheet operation.")
                .Build()),
            StylesheetOperationStatus.InvalidFileExtension => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file extension")
                .WithDetail("The file extension is not valid for a stylesheet.")
                .Build()),
            StylesheetOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            StylesheetOperationStatus.PathTooLong => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Path too long")
                .WithDetail("The file path is too long.")
                .Build()),
            StylesheetOperationStatus.NotFound => StylesheetNotFound(),
            StylesheetOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The stylesheet name is invalid.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown stylesheet operation status.")
                .Build()),
        };

    protected IActionResult StylesheetNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("Stylesheet not found")
        .WithDetail("The stylesheet was not found.")
        .Build());
}
