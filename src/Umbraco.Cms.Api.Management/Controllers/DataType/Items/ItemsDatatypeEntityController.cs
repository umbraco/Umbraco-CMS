using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Entity;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Items;

public class ItemsDatatypeEntityController : DatatypeEntityControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _mapper;

    public ItemsDatatypeEntityController(IDataTypeService dataTypeService, IUmbracoMapper mapper)
    {
        _dataTypeService = dataTypeService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DataTypeEntityResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items([FromQuery(Name = "key")] Guid[] keys)
    {
        var dataTypes = new List<IDataType>();
        foreach (Guid key in keys)
        {
            IDataType? dataType = await _dataTypeService.GetAsync(key);
            if (dataType is not null)
            {
                dataTypes.Add(dataType);
            }
        }

        List<DataTypeEntityResponseModel> responseModels = _mapper.MapEnumerable<IDataType, DataTypeEntityResponseModel>(dataTypes);
        return Ok(responseModels);
    }
}
