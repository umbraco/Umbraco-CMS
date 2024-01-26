using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Api.Management.Filters;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Install;

[AllowAnonymous]
[ApiController]
[VersionedApiBackOfficeRoute("install")]
[ApiExplorerSettings(GroupName = "Install")]
[RequireRuntimeLevel(RuntimeLevel.Install)]
public abstract class InstallControllerBase : ManagementApiControllerBase
{
    protected IActionResult InstallOperationResult(InstallOperationStatus status) =>
        status switch
        {
            InstallOperationStatus.Success => Ok(),
            InstallOperationStatus.UnknownDatabaseProvider => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid database configuration")
                .WithDetail("The database provider is unknown.")
                .Build()),
            InstallOperationStatus.MissingConnectionString => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid database configuration")
                .WithDetail("The connection string is missing.")
                .Build()),
            InstallOperationStatus.MissingProviderName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid database configuration")
                .WithDetail("The provider name is missing.")
                .Build()),
            InstallOperationStatus.DatabaseConnectionFailed => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid database configuration")
                .WithDetail("Could not connect to the database.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown install operation status.")
                .Build()),
        };
}
