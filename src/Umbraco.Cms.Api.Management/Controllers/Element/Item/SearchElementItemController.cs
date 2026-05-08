using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Item;

/// <summary>
/// Controller responsible for handling search operations for element items in the management API.
/// </summary>
[ApiVersion("1.0")]
public class SearchElementItemController : ElementItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IEntityService _entityService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchElementItemController"/> class, which handles search operations for element items.
    /// </summary>
    /// <param name="entitySearchService">Service used to perform entity search operations.</param>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public SearchElementItemController(
        IEntitySearchService entitySearchService,
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _entitySearchService = entitySearchService;
        _entityService = entityService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    /// Searches for element items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter element items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{ElementItemResponseModel}"/> containing the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<ElementItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches element items.")]
    [EndpointDescription("Searches element items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.Element, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<ElementItemResponseModel> { Total = searchResult.Total });
        }

        Guid[] keys = searchResult.Items.Select(item => item.Key).ToArray();
        IElementEntitySlim[] elements = _entityService
            .GetAll(UmbracoObjectTypes.Element, keys)
            .OfType<IElementEntitySlim>()
            .ToArray();

        ElementItemResponseModel[] items = await Task.WhenAll(elements.Select(_elementPresentationFactory.CreateItemResponseModelAsync));

        return Ok(
            new PagedModel<ElementItemResponseModel>
            {
                Items = items,
                Total = searchResult.Total,
            });
    }
}
