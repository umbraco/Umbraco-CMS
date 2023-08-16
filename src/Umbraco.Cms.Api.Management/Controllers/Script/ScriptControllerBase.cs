using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessScripts)]
public class ScriptControllerBase : ManagementApiControllerBase
{
    protected IActionResult ScriptOperationStatusResult(ScriptOperationStatus status) =>
        status switch
        {
            ScriptOperationStatus.Success => Ok(),
            ScriptOperationStatus.AlreadyExists => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Script already exists")
                .WithDetail("A script with the same path already exists")
                .Build()),
            ScriptOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A script notification handler prevented the script operation.")
                .Build()),
            ScriptOperationStatus.InvalidFileExtension => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid file extension")
                .WithDetail("The file extension is not valid for a script.")
                .Build()),
            ScriptOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Parent not found")
                .WithDetail("The parent folder was not found.")
                .Build()),
            ScriptOperationStatus.PathTooLong => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Path too long")
                .WithDetail("The file path is too long.")
                .Build()),
            ScriptOperationStatus.NotFound => ScriptNotFound(),
            ScriptOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The script name is invalid.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown script operation status.")
                .Build()),
        };

    protected IActionResult ScriptNotFound() => NotFound(new ProblemDetailsBuilder()
        .WithTitle("The script could not be found")
        .WithDetail("The script was not found.")
        .Build());

}
