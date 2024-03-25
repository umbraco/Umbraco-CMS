using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

[ApiVersion("1.0")]
[AllowAnonymous]
public class PublicPackageManifestController : PackageControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;

    public PublicPackageManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
    {
        _packageManifestService = packageManifestService;
        _umbracoMapper = umbracoMapper;
    }

    // NOTE: this endpoint is deliberately created as non-paginated to ensure the fastest possible client initialization
    [HttpGet("manifest/public")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PackageManifestResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PublicPackageManifests()
    {
        PackageManifest[] packageManifests = (await _packageManifestService.GetPublicPackageManifestsAsync()).ToArray();
        return Ok(_umbracoMapper.MapEnumerable<PackageManifest, PackageManifestResponseModel>(packageManifests));
    }
}
