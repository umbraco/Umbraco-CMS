using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}/folder")]
[ApiExplorerSettings(GroupName = "Script")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptFolderControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult OperationStatusResult(ScriptFolderOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ScriptFolderOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            ScriptFolderOperationStatus.NotEmpty => BadRequest(problemDetailsBuilder
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            ScriptFolderOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            ScriptFolderOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            ScriptFolderOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown script folder operation status.")
                .Build()),
        });
}
