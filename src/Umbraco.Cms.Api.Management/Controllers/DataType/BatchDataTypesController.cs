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
public class BatchDataTypesController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchDataTypesController"/> class.
    /// </summary>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="umbracoMapper">The presentation model mapper.</param>
    public BatchDataTypesController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("batch")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(BatchResponseModel<DataTypeResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets multiple data types.")]
    [EndpointDescription("Gets multiple data types identified by the provided Ids.")]
    public async Task<IActionResult> Batch(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        Guid[] requestedIds = [.. ids];

        if (requestedIds.Length == 0)
        {
            return Ok(new BatchResponseModel<DataTypeResponseModel>());
        }

        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetAllAsync(requestedIds);

        List<IDataType> ordered = OrderByRequestedIds(dataTypes, requestedIds);

        var responseModels = ordered.Select(dt => _umbracoMapper.Map<DataTypeResponseModel>(dt)!).ToList();

        return Ok(new BatchResponseModel<DataTypeResponseModel>
        {
            Total = responseModels.Count,
            Items = responseModels,
        });
    }
}
