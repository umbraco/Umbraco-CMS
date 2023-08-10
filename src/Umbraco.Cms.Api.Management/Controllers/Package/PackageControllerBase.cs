using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiController]
[VersionedApiBackOfficeRoute("package")]
[ApiExplorerSettings(GroupName = "Package")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessPackages)]
public abstract class PackageControllerBase : ManagementApiControllerBase
{
    protected IActionResult PackageOperationStatusResult(PackageOperationStatus status) =>
        status switch
        {
            PackageOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The package could not be found")
                    .Build()),
            PackageOperationStatus.DuplicateItemName => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Duplicate package name")
                .WithDetail("Another package already exists with the attempted name.")
                .Build()),
            PackageOperationStatus.InvalidName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Invalid package name")
                .WithDetail("The attempted package name does not represent a valid name for a package.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown package operation status.")
                .Build()),
        };

    protected IActionResult PackageMigrationOperationStatusResult(PackageMigrationOperationStatus status) =>
        status switch
        {
            PackageMigrationOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("No migration plans were found for that package")
                    .Build()),
            PackageMigrationOperationStatus.CancelledByFailedMigration => Conflict(new ProblemDetailsBuilder()
                .WithTitle("Package migration failed")
                .WithDetail("Check log for full details about the failed migration.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown package migration operation status.")
                .Build()),
        };
}
