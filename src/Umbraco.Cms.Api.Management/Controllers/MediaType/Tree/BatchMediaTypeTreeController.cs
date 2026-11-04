using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

/// <summary>
/// API controller responsible for retrieving multiple media type tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchMediaTypeTreeController : MediaTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchMediaTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for media type tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities in the tree.</param>
    /// <param name="idKeyMap">Map between entity IDs and keys.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    public BatchMediaTypeTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IMediaTypeService mediaTypeService)
        : base(
            entityService,
            flagProviders,
            entitySearchService,
            idKeyMap,
            mediaTypeService)
    {
    }

    /// <summary>
    /// Gets the media type tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the media type tree items to retrieve.</param>
    /// <returns>The media type tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple media type tree items.")]
    [EndpointDescription("Gets the media type tree items identified by the provided Ids.")]
    public async Task<ActionResult<BatchResponseModel<MediaTypeTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
        => await GetBatch([.. ids]);
}
