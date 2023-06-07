using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Items;

[ApiVersion("1.0")]
public class ItemDatatypeItemController : DatatypeItemControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    public ItemDatatypeItemController(IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        var dataTypes = new List<IDataType>();
        foreach (Guid id in ids)
        {
            IDataType? dataType = await _dataTypeService.GetAsync(id);
            if (dataType is not null)
            {
                dataTypes.Add(dataType);
            }
        }

        List<DataTypeItemResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeItemResponseModel>(dataTypes);
        return Ok(responseModels);
    }
}
