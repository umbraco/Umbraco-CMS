using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class AllCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllCreatedPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all created packages.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the created packages.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PackageDefinitionViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<PackageDefinitionViewModel>>> All(int skip = 0, int take = 100)
    {
        IEnumerable<PackageDefinition> createdPackages = _packagingService
            .GetAllCreatedPackages()
            .WhereNotNull()
            .Skip(skip)
            .Take(take);

        return await Task.FromResult(Ok(_umbracoMapper.Map<PagedViewModel<PackageDefinitionViewModel>>(createdPackages)));
    }
}
