using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[VersionedApiBackOfficeRoute("package")]
[ApiExplorerSettings(GroupName = "Package")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessPackages)]
public abstract class PackageControllerBase : ManagementApiControllerBase
{
    protected IActionResult PackageOperationStatusResult(PackageOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            PackageOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The package could not be found")
                .Build()),
            PackageOperationStatus.DuplicateItemName => Conflict(problemDetailsBuilder
                .WithTitle("Duplicate package name")
                .WithDetail("Another package already exists with the attempted name.")
                .Build()),
            PackageOperationStatus.InvalidName => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid package name")
                .WithDetail("The attempted package name does not represent a valid name for a package.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown package operation status.")
                .Build()),
        });

    protected IActionResult PackageMigrationOperationStatusResult(PackageMigrationOperationStatus status) =>
        OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            PackageMigrationOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("No migration plans were found for that package")
                .Build()),
            PackageMigrationOperationStatus.CancelledByFailedMigration => Conflict(problemDetailsBuilder
                .WithTitle("Package migration failed")
                .WithDetail("Check log for full details about the failed migration.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown package migration operation status.")
                .Build()),
        });
}
