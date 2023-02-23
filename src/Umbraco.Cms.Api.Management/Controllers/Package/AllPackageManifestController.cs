using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class AllPackageManifestController : PackageControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllPackageManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
    {
        _packageManifestService = packageManifestService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("manifest")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<PackageManifestViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<PackageManifestViewModel>>> AllPackageManifests(int skip = 0, int take = 100)
    {
        PackageManifest[] packageManifests = (await _packageManifestService.GetPackageManifestsAsync()).ToArray();
        return Ok(
            new PagedViewModel<PackageManifestViewModel>
            {
                Items = _umbracoMapper.MapEnumerable<PackageManifest, PackageManifestViewModel>(packageManifests.Skip(skip).Take(take)),
                Total = packageManifests.Length
            });
    }
}
