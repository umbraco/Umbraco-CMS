using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Manifest;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Web.Common.Hosting;

namespace Umbraco.Cms.Api.Management.Controllers.Manifest;

/// <summary>
/// Provides API endpoints for managing and retrieving public manifest information.
/// </summary>
[ApiVersion("1.0")]
[AllowAnonymous]
public class PublicManifestManifestController : ManifestControllerBase
{
    private readonly IPackageManifestService _packageManifestService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IBackOfficePathGenerator _backOfficePathGenerator;

/// <summary>
/// Initializes a new instance of the <see cref="PublicManifestManifestController"/> class with the specified services.
/// </summary>
/// <param name="packageManifestService">Service for managing package manifests.</param>
/// <param name="umbracoMapper">The mapper used for Umbraco object mapping.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PublicManifestManifestController(IPackageManifestService packageManifestService, IUmbracoMapper umbracoMapper)
        : this(
            packageManifestService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficePathGenerator>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicManifestManifestController"/> class.
    /// </summary>
    /// <param name="packageManifestService">Service for managing package manifests.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    /// <param name="backOfficePathGenerator">Generates paths for the back office.</param>
    [ActivatorUtilitiesConstructor]
    public PublicManifestManifestController(
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
    /// Retrieves the complete collection of public package manifests available to all users.
    /// </summary>
    /// <remarks>
    /// This endpoint is intentionally non-paginated to ensure the fastest possible client initialization.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a collection of <see cref="ManifestResponseModel"/> representing the public manifests.</returns>
    [HttpGet("manifest/public")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ManifestResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets public manifests.")]
    [EndpointDescription("Gets a collection of public package manifests available to all users.")]
    public async Task<IActionResult> PublicManifests(CancellationToken cancellationToken)
    {
        IEnumerable<PackageManifest> packageManifests = await _packageManifestService.GetPublicPackageManifestsAsync();
        IEnumerable<ManifestResponseModel> models = _umbracoMapper.MapEnumerable<PackageManifest, ManifestResponseModel>(packageManifests);
        ReplaceCacheBusterTokens(models, _backOfficePathGenerator.BackOfficeCacheBustHash);
        return Ok(models);
    }
}
