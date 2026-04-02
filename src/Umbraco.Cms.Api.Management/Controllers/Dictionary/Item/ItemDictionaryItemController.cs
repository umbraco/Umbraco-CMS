using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

/// <summary>
/// Provides API endpoints for managing individual dictionary items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemDictionaryItemController : DictionaryItemControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemDictionaryItemController"/> class with the specified services.
    /// </summary>
    /// <param name="dictionaryItemService">An instance of <see cref="IDictionaryItemService"/> used to manage dictionary items.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping objects.</param>
    public ItemDictionaryItemController(IDictionaryItemService dictionaryItemService, IUmbracoMapper mapper)
    {
        _dictionaryItemService = dictionaryItemService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a collection of dictionary items matching the specified IDs.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="ids">A set of dictionary item IDs to retrieve.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="IActionResult"/> with the collection of dictionary items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DictionaryItemItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of dictionary items.")]
    [EndpointDescription("Gets a collection of dictionary items identified by the provided Ids.")]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<DictionaryItemItemResponseModel>());
        }

        IEnumerable<IDictionaryItem> dictionaryItems = await _dictionaryItemService.GetManyAsync(ids.ToArray());

        List<DictionaryItemItemResponseModel> responseModels = _mapper.MapEnumerable<IDictionaryItem, DictionaryItemItemResponseModel>(dictionaryItems);
        return Ok(responseModels);
    }
}
