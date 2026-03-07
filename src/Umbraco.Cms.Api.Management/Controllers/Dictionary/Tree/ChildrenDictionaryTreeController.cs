using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Tree;

/// <summary>
/// API controller responsible for managing and retrieving the child nodes of dictionary items in the Umbraco dictionary tree.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenDictionaryTreeController : DictionaryTreeControllerBase
{
/// <summary>
/// Initializes a new instance of the <see cref="ChildrenDictionaryTreeController"/> class.
/// </summary>
/// <param name="entityService">Service used for managing and retrieving entities within the system.</param>
/// <param name="dictionaryItemService">Service used for managing dictionary items.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDictionaryTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within Umbraco.</param>
    /// <param name="flagProviders">A collection of providers that supply additional flags or metadata for entities.</param>
    /// <param name="dictionaryItemService">Service for managing dictionary items used for localization.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenDictionaryTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of dictionary tree items that are direct children of the specified parent dictionary item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent dictionary item whose children are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A paged view model containing the child dictionary tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of dictionary tree child items.")]
    [EndpointDescription("Gets a paginated collection of dictionary tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100)
    {
        PagedModel<IDictionaryItem> paginatedItems = await DictionaryItemService.GetPagedAsync(parentId, skip, take);

        return Ok(PagedViewModel(await MapTreeItemViewModels(paginatedItems.Items), paginatedItems.Total));
    }
}
