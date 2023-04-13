using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsUsed(Guid id)
    {
        Attempt<bool, DataTypeOperationStatus> result = await _dataTypeUsageService.HasSavedValuesAsync(id);

        return result.Success
            ? Ok(result.Result)
            : DataTypeOperationStatusResult(result.Status);
    }
}
