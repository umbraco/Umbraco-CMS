using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiVersion("1.0")]
public class AllMigrationStatusPackageController : PackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IPackagePresentationFactory _packagePresentationFactory;

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in V18.")]
    public AllMigrationStatusPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
        : this(packagingService, StaticServiceProvider.Instance.GetRequiredService<IPackagePresentationFactory>())
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in V18.")]
    public AllMigrationStatusPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper, IPackagePresentationFactory packagePresentationFactory)
        : this(packagingService, packagePresentationFactory)
    {
    }

    [ActivatorUtilitiesConstructor]
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
