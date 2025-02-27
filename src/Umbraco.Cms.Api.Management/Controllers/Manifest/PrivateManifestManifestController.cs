using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class PrivateManifestManifestController : ManifestControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;

    public PrivateManifestManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
    {
        _packageManifestService = packageManifestService;
        _umbracoMapper = umbracoMapper;
    }

    // NOTE: this endpoint is deliberately created as non-paginated to ensure the fastest possible client initialization
    [HttpGet("manifest/private")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ManifestResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PrivateManifests()
    {
        IEnumerable<PackageManifest> packageManifests = await _packageManifestService.GetPrivatePackageManifestsAsync();
        return Ok(_umbracoMapper.MapEnumerable<PackageManifest, ManifestResponseModel>(packageManifests));
    }
}
