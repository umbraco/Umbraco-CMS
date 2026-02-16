using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[ApiVersion("1.0")]
public class SearchMediaTypeItemController : MediaTypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IUmbracoMapper _mapper;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchMediaTypeItemController(
        IEntitySearchService entitySearchService,
        IMediaTypeService mediaTypeService,
        IUmbracoMapper mapper,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _entitySearchService = entitySearchService;
        _mediaTypeService = mediaTypeService;
        _mapper = mapper;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchMediaTypeItemController(IEntitySearchService entitySearchService, IMediaTypeService mediaTypeService, IUmbracoMapper mapper)
        : this(
            entitySearchService,
            mediaTypeService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchMediaTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches media type items.")]
    [EndpointDescription("Searches media type items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.MediaType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<SearchMediaTypeItemResponseModel> { Total = searchResult.Total });
        }

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(searchResult.Items, UmbracoObjectTypes.MediaType);

        IEnumerable<IMediaType> mediaTypes = _mediaTypeService.GetMany(searchResult.Items.Select(item => item.Key).ToArray().EmptyNull());
        IEnumerable<SearchMediaTypeItemResponseModel> searchModels = _mapper.MapEnumerable<IMediaType, SearchMediaTypeItemResponseModel>(mediaTypes);

        foreach (SearchMediaTypeItemResponseModel model in searchModels)
        {
            if (ancestorsByKey.TryGetValue(model.Id, out IReadOnlyList<SearchResultAncestorModel>? ancestors))
            {
                model.Ancestors = ancestors;
            }
        }

        var result = new PagedModel<SearchMediaTypeItemResponseModel>
        {
            Items = searchModels,
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
