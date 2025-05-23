using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
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

    public SearchDataTypeItemController(IEntitySearchService entitySearchService, IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _entitySearchService = entitySearchService;
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    [HttpGet("search")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedModel<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
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
