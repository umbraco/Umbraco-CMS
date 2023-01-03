using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

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
    [ProducesResponseType(typeof(DataTypeReferenceViewModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataTypeViewModel>> References(Guid key)
    {
        IDataType? dataType = _dataTypeService.GetDataType(key);
        if (dataType == null)
        {
            return NotFound();
        }

        IReadOnlyDictionary<Udi, IEnumerable<string>> usages = _dataTypeService.GetReferences(dataType.Id);
        DataTypeReferenceViewModel[] viewModels = _dataTypeReferenceViewModelFactory.CreateDataTypeReferenceViewModels(usages).ToArray();

        return await Task.FromResult(Ok(viewModels));
    }
}
