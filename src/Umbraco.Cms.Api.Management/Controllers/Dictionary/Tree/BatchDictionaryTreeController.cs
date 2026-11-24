using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

/// <summary>
/// API controller responsible for retrieving multiple dictionary tree items by key.
/// </summary>
[ApiVersion("1.0")]
public class BatchDictionaryTreeController : DictionaryTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BatchDictionaryTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities in the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for dictionary tree nodes.</param>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    public BatchDictionaryTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    /// <summary>
    /// Gets the dictionary tree items identified by the provided Ids.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="ids">The Ids of the dictionary tree items to retrieve.</param>
    /// <returns>The dictionary tree items, in the requested order.</returns>
    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple dictionary tree items.")]
    [EndpointDescription("Gets the dictionary tree items identified by the provided Ids.")]
    public async Task<ActionResult<BatchResponseModel<NamedEntityTreeItemResponseModel>>> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];
        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<NamedEntityTreeItemResponseModel>());
        }

        IEnumerable<IDictionaryItem> dictionaryItems = await DictionaryItemService.GetManyAsync(requestedIds);
        NamedEntityTreeItemResponseModel[] viewModels =
            [.. await MapTreeItemViewModels(OrderByRequestedIds(dictionaryItems, requestedIds))];

        return Ok(new BatchResponseModel<NamedEntityTreeItemResponseModel>
        {
            Total = viewModels.Length,
            Items = viewModels,
        });
    }
}
