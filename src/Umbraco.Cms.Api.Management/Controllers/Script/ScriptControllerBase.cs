using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = AuthorizationPolicies.TreeAccessScripts)]
public class ScriptControllerBase : FileSystemManagementControllerBase
{
    protected IActionResult ScriptOperationStatusResult(ScriptOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ScriptOperationStatus.AlreadyExists => BadRequest(problemDetailsBuilder
                .WithTitle("Script already exists")
                .WithDetail("A script with the same path already exists")
                .Build()),
            ScriptOperationStatus.CancelledByNotification => BadRequest(problemDetailsBuilder
                .WithTitle("Cancelled by notification")
                .WithDetail("A script notification handler prevented the script operation.")
                .Build()),
            ScriptOperationStatus.InvalidFileExtension => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid file extension")
                .WithDetail("The file extension is not valid for a script.")
                .Build()),
            ScriptOperationStatus.ParentNotFound => NotFound(problemDetailsBuilder
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            ScriptOperationStatus.PathTooLong => BadRequest(problemDetailsBuilder
                .WithTitle("Path too long")
                .WithDetail("The file path is too long.")
                .Build()),
            ScriptOperationStatus.NotFound => ScriptNotFound(),
            ScriptOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid name")
                .WithDetail("The script name is invalid.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown script operation status.")
                .Build()),
        });

    protected IActionResult ScriptNotFound() => OperationStatusResult(ScriptOperationStatus.NotFound, ScriptNotFound);

    private IActionResult ScriptNotFound(ProblemDetailsBuilder problemDetailsBuilder) => NotFound(problemDetailsBuilder
        .WithTitle("The script could not be found")
        .WithDetail("The script was not found.")
        .Build());
}
