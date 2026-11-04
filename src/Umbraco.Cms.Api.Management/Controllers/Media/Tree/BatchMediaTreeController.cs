using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

/// <summary>
/// API controller responsible for retrieving multiple media tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchMediaTreeController : MediaTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchMediaTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for media tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering media tree entities based on user start nodes.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    public BatchMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(
            entityService,
            flagProviders,
            treeFilterService,
            mediaPresentationFactory)
    {
    }

    /// <summary>
    /// Gets the media tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the media tree items to retrieve.</param>
    /// <param name="dataTypeId">The optional Id of a data type whose configuration may bypass user start nodes.</param>
    /// <returns>The media tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple media tree items.")]
    [EndpointDescription("Gets the media tree items identified by the provided Ids, omitting any the current user cannot see in the tree.")]
    public async Task<ActionResult<BatchResponseModel<MediaTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetBatch([.. ids]);
    }
}
