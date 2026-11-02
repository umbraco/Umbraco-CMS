using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

/// <summary>
/// API controller responsible for retrieving multiple document tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchDocumentTreeController : DocumentTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    public BatchDocumentTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IDocumentStartNodeTreeFilterService treeFilterService,
        IPublicAccessService publicAccessService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentPermissionFilterService documentPermissionFilterService)
        : base(
            entityService,
            flagProviders,
            treeFilterService,
            publicAccessService,
            documentPresentationFactory,
            documentPermissionFilterService)
    {
    }

    /// <summary>
    /// Gets the document tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the document tree items to retrieve.</param>
    /// <param name="dataTypeId">The optional Id of a data type whose configuration may bypass user start nodes.</param>
    /// <returns>The document tree items, in the requested order, omitting those the user cannot see in the tree.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple document tree items.")]
    [EndpointDescription("Gets the document tree items identified by the provided Ids, omitting any the current user cannot see in the tree.")]
    public async Task<ActionResult<BatchResponseModel<DocumentTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetBatch([.. ids]);
    }
}
