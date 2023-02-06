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
    ///     Gets a package by key.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <returns>The package or not found result.</returns>
    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PackageDefinitionViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageDefinitionViewModel>> ByKey(Guid key)
    {
        PackageDefinition? package = _packagingService.GetCreatedPackageByKey(key);

        if (package is null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(_umbracoMapper.Map<PackageDefinitionViewModel>(package)));
    }
}
