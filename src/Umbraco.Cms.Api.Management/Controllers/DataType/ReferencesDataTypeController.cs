using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

public class ReferencesDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IDataTypeReferenceViewModelFactory _dataTypeReferenceViewModelFactory;

    public ReferencesDataTypeController(IDataTypeService dataTypeService, IDataTypeReferenceViewModelFactory dataTypeReferenceViewModelFactory)
    {
        _dataTypeService = dataTypeService;
        _dataTypeReferenceViewModelFactory = dataTypeReferenceViewModelFactory;
    }

    [HttpGet("{key:guid}/references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeReferenceResponseModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> References(Guid key)
    {
        Attempt<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus> result = await _dataTypeService.GetReferencesAsync(key);
        if (result.Success == false)
        {
            return DataTypeOperationStatusResult(result.Status);
        }

        DataTypeReferenceResponseModel[] viewModels = _dataTypeReferenceViewModelFactory.CreateDataTypeReferenceViewModels(result.Result).ToArray();
        return Ok(viewModels);
    }
}
