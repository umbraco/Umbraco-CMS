using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Item;

    /// <summary>
    /// Controller responsible for handling search operations for data type items in the management API.
    /// </summary>
[ApiVersion("1.0")]
public class SearchDataTypeItemController : DatatypeItemControllerBase
{
    private readonly IEntitySearchService _entitySearchService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchDataTypeItemController"/> class, which handles search operations for data type items.
    /// </summary>
    /// <param name="entitySearchService">Service used to perform entity search operations.</param>
    /// <param name="dataTypeService">Service for managing data types.</param>
    /// <param name="mapper">The mapper used to convert between domain and API models.</param>
    public SearchDataTypeItemController(IEntitySearchService entitySearchService, IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    /// <summary>
    /// Searches for data type items matching the specified query, with support for pagination.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="query">The search query used to filter data type items.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedModel{DataTypeItemResponseModel}"/> containing the search results.</returns>
    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Searches data type items.")]
    [EndpointDescription("Searches data type items by the provided query with pagination support.")]
    public async Task<IActionResult> Search(CancellationToken cancellationToken, string query, int skip = 0, int take = 100)
    {
        PagedModel<IEntitySlim> searchResult = _entitySearchService.Search(UmbracoObjectTypes.DataType, query, skip, take);
        if (searchResult.Items.Any() is false)
        {
            return Ok(new PagedModel<DataTypeItemResponseModel> { Total = searchResult.Total });
        }

        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetAllAsync(searchResult.Items.Select(item => item.Key).ToArray());
        var result = new PagedModel<DataTypeItemResponseModel>
        {
            Items = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes),
            Total = searchResult.Total
        };

        return Ok(result);
    }
}
