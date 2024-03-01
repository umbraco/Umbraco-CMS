using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Filter;

[ApiVersion("1.0")]
public class FilterDataTypeFilterController : DataTypeFilterControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    public FilterDataTypeFilterController(IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter(
        int skip = 0,
        int take = 100,
        string name = "",
        string? editorUiAlias = null,
        string? editorAlias = null)
    {
        IEnumerable<IDataType> dataTypes = (await _dataTypeService.FilterAsync(name, editorUiAlias, editorAlias)).Skip(skip).Take(take);
        List<DataTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes);
        return Ok(responseModels);
    }
}
