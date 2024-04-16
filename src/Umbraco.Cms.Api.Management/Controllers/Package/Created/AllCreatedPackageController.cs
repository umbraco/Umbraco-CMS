using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

[ApiVersion("1.0")]
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
    [ProducesResponseType(typeof(PagedViewModel<PackageDefinitionResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<PackageDefinitionResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        PagedModel<PackageDefinition> createdPackages = await _packagingService.GetCreatedPackagesAsync(skip, take);

        var viewModel = new PagedViewModel<PackageDefinitionResponseModel>
        {
            Total = createdPackages.Total,
            Items = _umbracoMapper.MapEnumerable<PackageDefinition, PackageDefinitionResponseModel>(createdPackages.Items)
        };

        return await Task.FromResult(Ok(viewModel));
    }
}
