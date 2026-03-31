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

/// <summary>
/// Controller responsible for retrieving all manifest resources in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
public class AllManifestController : ManifestControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficePathGenerator _backOfficePathGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Manifest.AllManifestController"/> class, which manages operations related to all package manifests.
    /// </summary>
    /// <param name="packageManifestService">Service used to interact with package manifests.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public AllManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
        : this(
            packageManifestService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficePathGenerator>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllManifestController"/> class.
    /// </summary>
    /// <param name="packageManifestService">Service for managing package manifests.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    /// <param name="backOfficePathGenerator">Generates paths for the back office.</param>
    [ActivatorUtilitiesConstructor]
    public AllManifestController(
        IPackageManifestService packageManifestService,
        IUmbracoMapper umbracoMapper,
        IBackOfficePathGenerator backOfficePathGenerator)
    {
        _packageManifestService = packageManifestService;
        _umbracoMapper = umbracoMapper;
        _backOfficePathGenerator = backOfficePathGenerator;
    }

    // NOTE: this endpoint is deliberately created as non-paginated to ensure the fastest possible client initialization
    /// <summary>
    /// Retrieves all package manifests, including both public and private manifests, in a single non-paginated collection.
    /// This endpoint is optimized for fast client initialization and is not paginated by design.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing an <see cref="IEnumerable{ManifestResponseModel}"/> with all available package manifests.
    /// Returns <c>200 OK</c> with the collection on success.
    /// </returns>
    [HttpGet("manifest")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ManifestResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets all manifests.")]
    [EndpointDescription("Gets a collection of all package manifests including both public and private manifests.")]
    public async Task<IActionResult> AllManifests(CancellationToken cancellationToken)
    {
        IEnumerable<PackageManifest> packageManifests = await _packageManifestService.GetAllPackageManifestsAsync();
        IEnumerable<ManifestResponseModel> models = _umbracoMapper.MapEnumerable<PackageManifest, ManifestResponseModel>(packageManifests);
        ReplaceCacheBusterTokens(models, _backOfficePathGenerator.BackOfficeCacheBustHash);
        return Ok(models);
    }
}
