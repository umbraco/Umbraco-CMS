using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Items;

[ApiVersion("1.0")]
public class ByEditorUiAliasController : DatatypeItemControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    public ByEditorUiAliasController(
        IDataTypeService dataTypeService,
        IUmbracoMapper mapper)
    {
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    [HttpGet("item/{*alias}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeItemResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByEditorUiAlias(string alias)
    {
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorUiAlias(alias);
        List<DataTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes);
        return Ok(responseModels);
    }
}
