using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// Controller for checking whether a data type is currently in use.
/// </summary>
[ApiVersion("1.0")]
public class IsUsedDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeUsageService _dataTypeUsageService;

    public IsUsedDataTypeController(IDataTypeUsageService dataTypeUsageService)
    {
        _dataTypeUsageService = dataTypeUsageService;
    }

    /// <summary>
    /// Determines whether the data type specified by the given <paramref name="id"/> is currently used in any content, media, or member types.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the data type to check for usage.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a boolean value: <c>true</c> if the data type is used; <c>false</c> otherwise. Returns <see cref="StatusCodes.Status404NotFound"/> if the data type does not exist.
    /// </returns>
    [HttpGet("{id:guid}/is-used")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Checks if a data type is used.")]
    [EndpointDescription("Checks if the data type identified by the provided Id is used in any content, media, or member types.")]
    public async Task<IActionResult> IsUsed(CancellationToken cancellationToken, Guid id)
    {
        Attempt<bool, DataTypeOperationStatus> result = await _dataTypeUsageService.HasSavedValuesAsync(id);

        return result.Success
            ? Ok(result.Result)
            : DataTypeOperationStatusResult(result.Status);
    }
}
