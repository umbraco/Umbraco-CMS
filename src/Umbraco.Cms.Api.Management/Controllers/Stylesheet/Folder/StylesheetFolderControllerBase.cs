using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Stylesheet}/folder")]
[ApiExplorerSettings(GroupName = "Stylesheet")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessStylesheets)]
public class StylesheetFolderControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult OperationStatusResult(StylesheetFolderOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            StylesheetFolderOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            StylesheetFolderOperationStatus.NotEmpty => BadRequest(problemDetailsBuilder
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            StylesheetFolderOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            StylesheetFolderOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown stylesheet folder operation status.")
                .Build()),
        });
}
