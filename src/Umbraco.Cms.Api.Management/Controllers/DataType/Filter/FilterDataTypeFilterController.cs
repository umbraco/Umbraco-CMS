using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Language;
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
    [ProducesResponseType(typeof(PagedViewModel<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        string name = "",
        string? editorUiAlias = null,
        string? editorAlias = null)
    {
        PagedModel<IDataType> dataTypes = await _dataTypeService.FilterAsync(name, editorUiAlias, editorAlias, skip, take);
        List<DataTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes.Items);
        var viewModel = new PagedViewModel<DataTypeItemResponseModel>
        {
            Total = dataTypes.Total,
            Items = responseModels,
        };
        return Ok(viewModel);
    }
}
