using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

public class ByKeyCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyCreatedPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a package by id.
    /// </summary>
    /// <param name="id">The id of the package.</param>
    /// <returns>The package or not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PackageDefinitionResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageDefinitionResponseModel>> ByKey(Guid id)
    {
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(id);

        if (package is null)
        {
            return NotFound();
        }

        return Ok(_umbracoMapper.Map<PackageDefinitionResponseModel>(package));
    }
}
