using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller for managing data types by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyDataTypeController"/> class.
    /// </summary>
    /// <param name="dataTypeService">Service used for managing and retrieving data types.</param>
    /// <param name="umbracoMapper">The mapper used to map between Umbraco domain models and API models.</param>
    public ByKeyDataTypeController(IDataTypeService dataTypeService, IUmbracoMapper umbracoMapper)
    {
        _dataTypeService = dataTypeService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a data type by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the data type to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the data type if found; otherwise, a 404 Not Found result.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a data type.")]
    [EndpointDescription("Gets a data type identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        IDataType? dataType = await _dataTypeService.GetAsync(id);
        if (dataType == null)
        {
            return DataTypeNotFound();
        }

        return Ok(_umbracoMapper.Map<DataTypeResponseModel>(dataType));
    }
}
