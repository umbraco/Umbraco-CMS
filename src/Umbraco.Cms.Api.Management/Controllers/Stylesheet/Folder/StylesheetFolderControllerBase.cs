using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}/folder")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetFolderControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult OperationStatusResult(StylesheetFolderOperationStatus status) =>
        status switch
        {
            StylesheetFolderOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            StylesheetFolderOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            StylesheetFolderOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown stylesheet folder operation status.")
                .Build()),
        };
}
