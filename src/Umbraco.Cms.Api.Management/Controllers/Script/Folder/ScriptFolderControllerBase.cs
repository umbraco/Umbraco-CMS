using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}/folder")]
[ApiExplorerSettings(GroupName = "Script")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessScripts)]
public class ScriptFolderControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult OperationStatusResult(ScriptFolderOperationStatus status) =>
        status switch
        {
            ScriptFolderOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Folder already exists")
                .WithDetail("The folder already exists")
                .Build()),
            ScriptFolderOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Not empty")
                .WithDetail("The folder is not empty and can therefore not be deleted.")
                .Build()),
            ScriptFolderOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Not found")
                .WithDetail("The specified folder was not found.")
                .Build()),
            ScriptFolderOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            ScriptFolderOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The name specified is not a valid name.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown script folder operation status.")
                .Build()),
        };
}
