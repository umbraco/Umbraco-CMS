using System;
using System.Collections.Generic;
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPoco;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

/// <summary>
/// Controller responsible for handling search operations for dictionary items in the management API.
/// </summary>
[ApiVersion("1.0")]
public class SearchDictionaryItemController: DictionaryItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IEntityService _entityService;
    private readonly IUmbracoMapper _mapper;


    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDictionaryItemController"/> class, which handles search operations for dictionary items.
    /// </summary>
    /// <param name="entitySearchService">Service used to perform entity search operations.</param>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public SearchDictionaryItemController(
        IEntitySearchService entitySearchService,
        IEntityService entityService,
        IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _entityService = entityService;
        _mapper = mapper;
    }

    /// <summary>
    /// Searches for dictionary items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter dictionary items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{DictionaryItemResponseModel}"/> containing the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DictionaryItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches dictionary items.")]
    [EndpointDescription("Searches dictionary items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.DictionaryItem, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<DictionaryItemResponseModel> { Total = searchResult.Total });
        }

        Guid[] keys = searchResult.Items.Select(item => item.Key).ToArray();

        var dictionaryItemsByKey = _entityService
            .GetAll(UmbracoObjectTypes.DictionaryItem, keys)
            .OfType<IDictionaryItem>()
            .ToDictionary(e => e.Key);
        IDictionaryItem[] dictionaryItems = keys
            .Where(dictionaryItemsByKey.ContainsKey)
            .Select(key => dictionaryItemsByKey[key])
            .ToArray();

        return Ok(
            new PagedModel<DictionaryItemResponseModel>
            {
                Items = _mapper.MapEnumerable<IDictionaryItem, DictionaryItemResponseModel>(dictionaryItems),
                Total = searchResult.Total,
            });
    }
}
