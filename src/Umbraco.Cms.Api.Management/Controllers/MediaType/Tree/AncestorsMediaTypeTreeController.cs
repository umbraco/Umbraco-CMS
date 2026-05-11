using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

/// <summary>
/// Provides API endpoints for retrieving the ancestor tree structure of media types in the system.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsMediaTypeTreeController : MediaTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.Tree.AncestorsMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="mediaTypeService">Service used for managing media type definitions.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="mediaTypeService">Service used for managing media types in the CMS.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsMediaTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMediaTypeService mediaTypeService)
        : base(entityService, flagProviders, mediaTypeService)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor media type items.")]
    [EndpointDescription("Gets a collection of media type items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<MediaTypeTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
