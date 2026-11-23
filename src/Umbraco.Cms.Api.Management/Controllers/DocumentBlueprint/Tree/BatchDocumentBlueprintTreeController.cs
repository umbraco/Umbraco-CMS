using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

/// <summary>
/// API controller responsible for retrieving multiple document blueprint tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchDocumentBlueprintTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document blueprint tree nodes.</param>
    /// <param name="entitySearchService">Service for searching entities in the tree.</param>
    /// <param name="idKeyMap">Map between entity IDs and keys.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    public BatchDocumentBlueprintTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(
            entityService,
            flagProviders,
            entitySearchService,
            idKeyMap,
            documentPresentationFactory)
    {
    }

    /// <summary>
    /// Gets the document blueprint tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the document blueprint tree items to retrieve.</param>
    /// <returns>The document blueprint tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple document blueprint tree items.")]
    [EndpointDescription("Gets the document blueprint tree items identified by the provided Ids.")]
    public async Task<ActionResult<BatchResponseModel<DocumentBlueprintTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
        => await GetBatch([.. ids]);
}
