﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.Script}")]
[ApiExplorerSettings(GroupName = nameof(Constants.UdiEntityType.Script))]
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
            ScriptOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                .WithTitle("Script not found")
                .WithDetail("The script was not found.")
                .Build()),
            ScriptOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid name")
                .WithDetail("The script name is invalid.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown script operation status")
        };
}
