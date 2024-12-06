using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheetsOrDocumentOrMediaOrMember)]
public class StylesheetControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult StylesheetOperationStatusResult(StylesheetOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            StylesheetOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("Stylesheet already exists")
                .WithDetail("A stylesheet with the same path already exists")
                .Build()),
            StylesheetOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A stylesheet notification handler prevented the stylesheet operation.")
                .Build()),
            StylesheetOperationStatus.InvalidFileExtension => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid file extension")
                .WithDetail("The file extension is not valid for a stylesheet.")
                .Build()),
            StylesheetOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            StylesheetOperationStatus.PathTooLong => BadRequest(problemDetailsBuilder
                .WithTitle("Path too long")
                .WithDetail("The file path is too long.")
                .Build()),
            StylesheetOperationStatus.NotFound => StylesheetNotFound(problemDetailsBuilder),
            StylesheetOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid name")
                .WithDetail("The stylesheet name is invalid.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown stylesheet operation status.")
                .Build()),
        });

    protected IActionResult StylesheetNotFound() => OperationStatusResult(StylesheetOperationStatus.NotFound, StylesheetNotFound);

    protected IActionResult StylesheetNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("Stylesheet not found")
        .WithDetail("The stylesheet was not found.")
        .Build());
}
