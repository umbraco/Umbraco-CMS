using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

    /// <summary>
    /// API controller for managing Umbraco dictionary items by their unique key.
    /// Provides endpoints for retrieving, updating, and deleting dictionary entries identified by key.
    /// </summary>
[ApiVersion("1.0")]
public class ByKeyDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IDictionaryPresentationFactory _dictionaryPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDictionaryController"/> class, which manages dictionary items by key.
    /// </summary>
    /// <param name="dictionaryItemService">The <see cref="IDictionaryItemService"/> used to manage dictionary items.</param>
    /// <param name="dictionaryPresentationFactory">The <see cref="IDictionaryPresentationFactory"/> used to create dictionary item presentations.</param>
    public ByKeyDictionaryController(IDictionaryItemService dictionaryItemService, IDictionaryPresentationFactory dictionaryPresentationFactory)
    {
        _dictionaryItemService = dictionaryItemService;
        _dictionaryPresentationFactory = dictionaryPresentationFactory;
    }

    /// <summary>
    /// Retrieves a dictionary item by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the dictionary item to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the <see cref="DictionaryItemResponseModel"/> if the item is found;
    /// otherwise, a 404 Not Found response.
    /// </returns>
    [HttpGet($"{{{nameof(id)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryItemResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a dictionary.")]
    [EndpointDescription("Gets a dictionary identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IDictionaryItem? dictionary = await _dictionaryItemService.GetAsync(id);
        if (dictionary == null)
        {
            return DictionaryItemNotFound();
        }

        return Ok(await _dictionaryPresentationFactory.CreateDictionaryItemViewModelAsync(dictionary));
    }
}
