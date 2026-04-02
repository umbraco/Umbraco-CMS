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
/// Controller for managing the root of the dictionary tree in the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class RootDictionaryTreeController : DictionaryTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDictionaryTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the dictionary tree.</param>
    /// <param name="dictionaryItemService">Service used for managing dictionary items.</param>
    public RootDictionaryTreeController(IEntityService entityService, IDictionaryItemService dictionaryItemService)
        : base(entityService, dictionaryItemService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootDictionaryTreeController"/> class, which manages the root nodes of the dictionary tree in the Umbraco management API.
    /// </summary>
    /// <param name="entityService">Service for managing entities within Umbraco.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="dictionaryItemService">Service for managing dictionary items.</param>
    [ActivatorUtilitiesConstructor]
    public RootDictionaryTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDictionaryItemService dictionaryItemService)
        : base(entityService, flagProviders, dictionaryItemService)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of dictionary items from the root of the dictionary tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip for pagination. Defaults to 0.</param>
    /// <param name="take">The maximum number of items to return for pagination. Defaults to 100.</param>
    /// <returns>
    /// An <see cref="ActionResult{T}"/> containing a <see cref="PagedViewModel{NamedEntityTreeItemResponseModel}"/>,
    /// which represents the paginated dictionary items from the root of the tree.
    /// </returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of dictionary items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of dictionary items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Root(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        PagedModel<IDictionaryItem> paginatedItems = await DictionaryItemService.GetPagedAsync(null, skip, take);

        return Ok(PagedViewModel(await MapTreeItemViewModels(paginatedItems.Items), paginatedItems.Total));
    }
}
