using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiController]
[VersionedApiBackOfficeRoute("package")]
[ApiExplorerSettings(GroupName = "Package")]
[ApiVersion("1.0")]
public abstract class PackageControllerBase : ManagementApiControllerBase
{
    protected IActionResult PackageMigrationOperationStatusResult(PackageMigrationOperationStatus status) =>
        status switch
        {
            PackageMigrationOperationStatus.NotFound => NotFound("No migration plans were found for that package"),
            PackageMigrationOperationStatus.CancelledByFailedMigration => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Package migration failed")
                .WithDetail("Check log for full details about the failed migration.")
                .WithStatus(StatusCodes.Status409Conflict)
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "Unknown package migration operation status")
        };
}
