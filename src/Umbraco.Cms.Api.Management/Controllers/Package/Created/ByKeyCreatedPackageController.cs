using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Package.Created;

/// <summary>
/// Controller responsible for handling operations on packages that are identified and managed using a unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyCreatedPackageController : CreatedPackageControllerBase
{
    private readonly IPackagingService _packagingService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyCreatedPackageController"/> class.
    /// </summary>
    /// <param name="packagingService">Service used for package management operations.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco objects.</param>
    public ByKeyCreatedPackageController(IPackagingService packagingService, IUmbracoMapper umbracoMapper)
    {
        _packagingService = packagingService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a package by id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="id">The id of the package.</param>
    /// <returns>The package or not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(PackageDefinitionResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a package.")]
    [EndpointDescription("Gets a package identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        PackageDefinition? package = await _packagingService.GetCreatedPackageByKeyAsync(id);

        if (package is null)
        {
            return CreatedPackageNotFound();
        }

        return Ok(_umbracoMapper.Map<PackageDefinitionResponseModel>(package));
    }
}
