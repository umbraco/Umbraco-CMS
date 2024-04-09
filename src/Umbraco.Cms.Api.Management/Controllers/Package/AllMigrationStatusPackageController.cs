using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiVersion("1.0")]
public class AllMigrationStatusPackageController : PackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllMigrationStatusPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of the migration status of each installed package.
    /// </summary>
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

        var viewModel = new PagedViewModel<PackageMigrationStatusResponseModel>
        {
            Total = migrationPlans.Total,
            Items = _umbracoMapper.MapEnumerable<InstalledPackage, PackageMigrationStatusResponseModel>(migrationPlans.Items)
        };

        return Ok(viewModel);
    }
}
