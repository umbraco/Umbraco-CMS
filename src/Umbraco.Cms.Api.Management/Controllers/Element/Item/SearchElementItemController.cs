using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IIndexedEntitySearchService _indexedEntitySearchService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchElementItemController"/> class, which handles search operations for element items.
    /// </summary>
    /// <param name="indexedEntitySearchService">Service used to search the element index.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public SearchElementItemController(
        IIndexedEntitySearchService indexedEntitySearchService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _indexedEntitySearchService = indexedEntitySearchService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchElementItemController"/> class, which handles search operations for element items.
    /// </summary>
    /// <remarks>
    /// This constructor exists only to give the dependency injection container an unambiguous (greediest) constructor
    /// to select while the obsolete constructor remains. Use the constructor taking an <see cref="IIndexedEntitySearchService"/>.
    /// </remarks>
    [Obsolete("This constructor exists only to satisfy dependency injection. Please use the constructor taking an IIndexedEntitySearchService. Scheduled for removal in Umbraco 21.")]
    public SearchElementItemController(
        IEntitySearchService entitySearchService,
        IEntityService entityService,
        IIndexedEntitySearchService indexedEntitySearchService,
        IElementPresentationFactory elementPresentationFactory)
        : this(indexedEntitySearchService, elementPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchElementItemController"/> class, which handles search operations for element items.
    /// </summary>
    /// <param name="entitySearchService">Service used to perform entity search operations.</param>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    [Obsolete("Please use the constructor taking an IIndexedEntitySearchService. Scheduled for removal in Umbraco 21.")]
    public SearchElementItemController(
        IEntitySearchService entitySearchService,
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IIndexedEntitySearchService>(),
            elementPresentationFactory)
    {
    }

    /// <summary>
    /// Searches for element items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter element items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{ElementItemResponseModel}"/> containing the search results.</returns>
    [Obsolete("Please use SearchWithFilters. Scheduled for removal in Umbraco 21.")]
    [NonAction]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
        => await SearchWithFilters(cancellationToken, query, skip: skip, take: take);

    /// <summary>
    /// Searches for element items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter element items.</param>
    /// <param name="trashed">
    /// <c>true</c> to return only trashed elements, <c>false</c> to exclude them, or <c>null</c> to return both.
    /// </param>
    /// <param name="culture">
    /// The culture to search culture-variant elements in. Invariant elements are always searched.
    /// When omitted, only invariant elements are searched.
    /// </param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <param name="parentId">The ID of the element folder to restrict the search to. Optional.</param>
    /// <param name="allowedElementTypes">A list of allowed element type IDs to filter the search results by. Optional.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{ElementItemResponseModel}"/> containing the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<ElementItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches element items.")]
    [EndpointDescription("Searches element items by the provided query with pagination support.")]
    public async Task<IActionResult> SearchWithFilters(
        CancellationToken cancellationToken,
        string query,
        bool? trashed = null,
        string? culture = null,
        int skip = 0,
        int take = 100,
        Guid? parentId = null,
        [FromQuery] IEnumerable<Guid>? allowedElementTypes = null)
    {
        PagedModel<IEntitySlim> searchResult = await _indexedEntitySearchService.SearchAsync(
            UmbracoObjectTypes.Element,
            query,
            parentId,
            allowedElementTypes,
            trashed,
            culture,
            skip,
            take);

        ElementItemResponseModel[] items = await Task.WhenAll(
            searchResult.Items.OfType<IElementEntitySlim>().Select(_elementPresentationFactory.CreateItemResponseModelAsync));

        return Ok(
            new PagedModel<ElementItemResponseModel>
            {
                Items = items,
                Total = searchResult.Total,
            });
    }
}
