using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

/// <summary>
/// Controller for retrieving the migration status of all packages.
/// </summary>
[ApiVersion("1.0")]
public class AllMigrationStatusPackageController : PackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IPackagePresentationFactory _packagePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllMigrationStatusPackageController"/> class, which handles migration status operations for packages.
    /// </summary>
    /// <param name="packagingService">Service used for package-related operations.</param>
    /// <param name="packagePresentationFactory">Factory for creating package presentation models.</param>
    public AllMigrationStatusPackageController(IPackagingService packagingService, IPackagePresentationFactory packagePresentationFactory)
    {
        _packagingService = packagingService;
        _packagePresentationFactory = packagePresentationFactory;
    }

    /// <summary>
    ///     Gets a paginated list of the migration status of each installed package.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the installed packages migration status.</returns>
    [HttpGet("migration-status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PackageMigrationStatusResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets all package migration statuses.")]
    [EndpointDescription("Gets a paginated collection of migration status for all installed packages.")]
    public async Task<ActionResult<PagedViewModel<PackageMigrationStatusResponseModel>>> AllMigrationStatuses(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<InstalledPackage> migrationPlans = await _packagingService.GetInstalledPackagesFromMigrationPlansAsync(skip, take);

        PagedViewModel<PackageMigrationStatusResponseModel> viewModel = _packagePresentationFactory.CreatePackageMigrationStatusResponseModel(migrationPlans);

        return Ok(viewModel);
    }
}
