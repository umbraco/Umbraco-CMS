using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

[AllowAnonymous]
[VersionedApiBackOfficeRoute("install")]
[ApiExplorerSettings(GroupName = "Install")]
[RequireRuntimeLevel(RuntimeLevel.Install)]
public abstract class InstallControllerBase : ManagementApiControllerBase
{
    protected IActionResult InstallOperationResult(InstallOperationStatus status, InstallationResult? result = null) =>
        status is InstallOperationStatus.Success
            ? Ok()
            : OperationStatusResult(status, problemDetailsBuilder => status switch
            {
                InstallOperationStatus.UnknownDatabaseProvider => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid database configuration")
                    .WithDetail("The database provider is unknown.")
                    .Build()),
                InstallOperationStatus.MissingConnectionString => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid database configuration")
                    .WithDetail("The connection string is missing.")
                    .Build()),
                InstallOperationStatus.MissingProviderName => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid database configuration")
                    .WithDetail("The provider name is missing.")
                    .Build()),
                InstallOperationStatus.DatabaseConnectionFailed => BadRequest(problemDetailsBuilder
                    .WithTitle("Invalid database configuration")
                    .WithDetail("Could not connect to the database.")
                    .Build()),
                InstallOperationStatus.InstallFailed => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Install failed")
                    .WithDetail(result?.ErrorMessage ?? "An unknown error occurred.")
                    .Build()),
                _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                    .WithTitle("Unknown install operation status.")
                    .Build()),
            });
}
