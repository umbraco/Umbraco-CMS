using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiVersion("1.0")]
[Obsolete("Please use ReferencedByDataTypeController and the referenced-by endpoint. Scheduled for removal in Umbraco 17.")]
public class ReferencesDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IDataTypeReferencePresentationFactory _dataTypeReferencePresentationFactory;

    public ReferencesDataTypeController(IDataTypeService dataTypeService, IDataTypeReferencePresentationFactory dataTypeReferencePresentationFactory)
    {
        _dataTypeService = dataTypeService;
        _dataTypeReferencePresentationFactory = dataTypeReferencePresentationFactory;
    }

    [HttpGet("{id:guid}/references")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DataTypeReferenceResponseModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> References(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IReadOnlyDictionary<Udi, IEnumerable<string>>, DataTypeOperationStatus> result = await _dataTypeService.GetReferencesAsync(id);
        if (result.Success == false)
        {
            return DataTypeOperationStatusResult(result.Status);
        }

        DataTypeReferenceResponseModel[] viewModels = _dataTypeReferencePresentationFactory.CreateDataTypeReferenceViewModels(result.Result).ToArray();
        return Ok(viewModels);
    }
}
