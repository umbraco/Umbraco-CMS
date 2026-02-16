using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Item;

[ApiVersion("1.0")]
public class SearchDataTypeItemController : DatatypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;
    private readonly ISearchResultAncestorService _searchResultAncestorService;

    [ActivatorUtilitiesConstructor]
    public SearchDataTypeItemController(
        IEntitySearchService entitySearchService,
        IDataTypeService dataTypeService,
        IUmbracoMapper mapper,
        ISearchResultAncestorService searchResultAncestorService)
    {
        _entitySearchService = entitySearchService;
        _dataTypeService = dataTypeService;
        _mapper = mapper;
        _searchResultAncestorService = searchResultAncestorService;
    }

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public SearchDataTypeItemController(IEntitySearchService entitySearchService, IDataTypeService dataTypeService, IUmbracoMapper mapper)
        : this(
            entitySearchService,
            dataTypeService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<ISearchResultAncestorService>())
    {
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<SearchDataTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches data type items.")]
    [EndpointDescription("Searches data type items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.DataType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<SearchDataTypeItemResponseModel> { Total = searchResult.Total });
        }

        IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>> ancestorsByKey =
            await _searchResultAncestorService.ResolveAsync(searchResult.Items, UmbracoObjectTypes.DataType);

        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetAllAsync(searchResult.Items.Select(item => item.Key).ToArray());
        IEnumerable<SearchDataTypeItemResponseModel> searchModels = _mapper.MapEnumerable<IDataType, SearchDataTypeItemResponseModel>(dataTypes);

        foreach (SearchDataTypeItemResponseModel model in searchModels)
        {
            if (ancestorsByKey.TryGetValue(model.Id, out IReadOnlyList<SearchResultAncestorModel>? ancestors))
            {
                model.Ancestors = ancestors;
            }
        }

        var result = new PagedModel<SearchDataTypeItemResponseModel>
        {
            Items = searchModels,
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
