using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Hosting;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class PrivateManifestManifestController : ManifestControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficePathGenerator _backOfficePathGenerator;

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PrivateManifestManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
        : this(
            packageManifestService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficePathGenerator>())
    {
    }

    [ActivatorUtilitiesConstructor]
    public PrivateManifestManifestController(
        IPackageManifestService packageManifestService,
        IUmbracoMapper umbracoMapper,
        IBackOfficePathGenerator backOfficePathGenerator)
    {
        _packageManifestService = packageManifestService;
        _umbracoMapper = umbracoMapper;
        _backOfficePathGenerator = backOfficePathGenerator;
    }

    // NOTE: this endpoint is deliberately created as non-paginated to ensure the fastest possible client initialization
    [HttpGet("manifest/private")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ManifestResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets private manifests.")]
    [EndpointDescription("Gets a collection of private package manifests specific to the current user.")]
    public async Task<IActionResult> PrivateManifests()
    {
        IEnumerable<PackageManifest> packageManifests = await _packageManifestService.GetPrivatePackageManifestsAsync();
        IEnumerable<ManifestResponseModel> models = _umbracoMapper.MapEnumerable<PackageManifest, ManifestResponseModel>(packageManifests);
        ReplaceCacheBusterTokens(models, _backOfficePathGenerator.BackOfficeCacheBustHash);
        return Ok(models);
    }
}
