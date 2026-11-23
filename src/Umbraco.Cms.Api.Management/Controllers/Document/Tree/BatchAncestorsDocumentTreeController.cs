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
/// API controller responsible for retrieving the ancestors of multiple document tree items.
/// </summary>
[ApiVersion("1.0")]
public class BatchAncestorsDocumentTreeController : DocumentTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchAncestorsDocumentTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document tree nodes.</param>
    /// <param name="treeFilterService">Service for filtering document tree entities based on user start nodes.</param>
    /// <param name="publicAccessService">Service for handling public access permissions on documents.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    /// <param name="documentPermissionFilterService">Service for filtering documents based on user permissions.</param>
    public BatchAncestorsDocumentTreeController(
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
    /// Gets the ancestors of each of the document tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the document tree items to get the ancestors of.</param>
    /// <param name="dataTypeId">The optional Id of a data type whose configuration may bypass user start nodes.</param>
    /// <returns>
    /// The ancestors of each document tree item, in the requested order, omitting the items and ancestors the user
    /// cannot see in the tree.
    /// </returns>
    [HttpGet("ancestors/batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<DocumentTreeAncestorsResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the ancestors of multiple document tree items.")]
    [EndpointDescription("Gets the ancestors of each document tree item identified by the provided Ids, ordered from the root down and excluding the item itself. Items and ancestors the current user cannot see in the tree are omitted.")]
    public async Task<ActionResult<BatchResponseModel<DocumentTreeAncestorsResponseModel>>> AncestorsBatch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);

        DocumentTreeAncestorsResponseModel[] items = (await GetAncestorsOfItems([.. ids]))
            .Select(chain => new DocumentTreeAncestorsResponseModel
            {
                Id = chain.DescendantKey,
                Ancestors = chain.Ancestors,
            })
            .ToArray();

        return Ok(new BatchResponseModel<DocumentTreeAncestorsResponseModel>
        {
            Total = items.Length,
            Items = items,
        });
    }
}
