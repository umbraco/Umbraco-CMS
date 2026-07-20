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

/// <summary>
/// API controller responsible for managing and retrieving information about all packages created in the system.
/// </summary>
[ApiVersion("1.0")]
public class AllCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllCreatedPackageController"/> class.
    /// </summary>
    /// <param name="packagingService">Service used for package-related operations.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects.</param>
    public AllCreatedPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a paginated list of all created packages.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    /// <returns>The paged result of the created packages.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PackageDefinitionResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a paginated collection of created packages.")]
    [EndpointDescription("Gets a paginated collection of all created packages.")]
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

        return Ok(viewModel);
    }
}
