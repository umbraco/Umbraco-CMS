using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiVersion("1.0")]
public class IsUsedDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeUsageService _dataTypeUsageService;

    public IsUsedDataTypeController(IDataTypeUsageService dataTypeUsageService)
    {
        _dataTypeUsageService = dataTypeUsageService;
    }

    [HttpGet("{id:guid}/is-used")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsUsed(CancellationToken cancellationToken, Guid id)
    {
        Attempt<bool, DataTypeOperationStatus> result = await _dataTypeUsageService.HasSavedValuesAsync(id);

        return result.Success
            ? Ok(result.Result)
            : DataTypeOperationStatusResult(result.Status);
    }
}
