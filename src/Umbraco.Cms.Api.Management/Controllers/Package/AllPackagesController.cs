using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class AllPackagesController : PackageControllerBase
{
    private readonly IExtensionManifestService _extensionManifestService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllPackagesController(IExtensionManifestService extensionManifestService, IUmbracoMapper umbracoMapper)
    {
        _extensionManifestService = extensionManifestService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    // TODO: proper view model + mapper
    [ProducesResponseType(typeof(PagedViewModel<ExtensionManifestViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<ExtensionManifestViewModel>>> AllMigrationStatuses(int skip = 0, int take = 100)
    {
        ExtensionManifest[] extensionManifests = (await _extensionManifestService.GetManifestsAsync()).ToArray();
        return Ok(
            new PagedViewModel<ExtensionManifestViewModel>
            {
                Items = _umbracoMapper.MapEnumerable<ExtensionManifest, ExtensionManifestViewModel>(extensionManifests.Skip(skip).Take(take)),
                Total = extensionManifests.Length
            });
    }
}
