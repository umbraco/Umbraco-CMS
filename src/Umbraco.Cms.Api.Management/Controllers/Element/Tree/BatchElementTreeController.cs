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

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

/// <summary>
/// API controller responsible for retrieving multiple element tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchElementTreeController : ElementTreeControllerBase
{
    /// <inheritdoc />
    public BatchElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IEntitySearchService entitySearchService,
        IIdKeyMap idKeyMap,
        IElementStartNodeTreeFilterService treeFilterService,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, entitySearchService, idKeyMap, treeFilterService, elementPresentationFactory, elementPermissionFilterService)
    {
    }

    /// <summary>
    /// Gets the element tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the element or element folder tree items to retrieve.</param>
    /// <param name="dataTypeId">The optional Id of a data type whose configuration may bypass user start nodes.</param>
    /// <returns>The element tree items, in the requested order, omitting those the user cannot see in the tree.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple element tree items.")]
    [EndpointDescription("Gets the element tree items identified by the provided Ids, omitting any the current user cannot see in the tree.")]
    public async Task<ActionResult<BatchResponseModel<ElementTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetBatch([.. ids]);
    }
}
