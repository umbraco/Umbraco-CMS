using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class RunMigrationPackageController : PackageControllerBase
{
    private readonly PackageMigrationRunner _packageMigrationRunner;

    public RunMigrationPackageController(PackageMigrationRunner packageMigrationRunner)
        => _packageMigrationRunner = packageMigrationRunner;

    /// <summary>
    ///     Runs all migration plans for a package with a given name if any are pending.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <returns>The result of running the package migrations.</returns>
    [HttpPost("{packageName}/run-migration")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RunMigrations(string packageName)
    {
        Attempt<bool, PackageMigrationOperationStatus> result = await _packageMigrationRunner.RunningPendingPackageMigrationsSucceeded(packageName);

        if (result.Success)
        {
            return Ok();
        }

        return PackageMigrationOperationStatusResult(result.Status);
    }
}
