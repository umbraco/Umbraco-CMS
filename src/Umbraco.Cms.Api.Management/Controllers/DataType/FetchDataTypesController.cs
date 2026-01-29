using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Provides an API controller for retrieving the full details for multiple data types by key.
/// </summary>
[ApiVersion("1.0")]
public class FetchDataTypesController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FetchDataTypesController"/> class.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public FetchDataTypesController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("fetch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FetchResponseModel<DataTypeResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fetch(CancellationToken cancellationToken, FetchRequestModel requestModel)
    {
        Guid[] ids = [.. requestModel.Ids.Select(x => x.Id).Distinct()];

        if (ids.Length == 0)
        {
            return Ok(new FetchResponseModel<DataTypeResponseModel>());
        }

        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetAllAsync(ids);

        List<IDataType> ordered = OrderByRequestedIds(dataTypes, ids);

        var responseModels = ordered.Select(dt => _umbracoMapper.Map<DataTypeResponseModel>(dt)!).ToList();

        return Ok(new FetchResponseModel<DataTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
