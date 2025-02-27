using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiVersion("1.0")]
public class CreateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CreateDataTypeController(IDataTypeService dataTypeService, IDataTypePresentationFactory dataTypePresentationFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _dataTypePresentationFactory = dataTypePresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateDataTypeRequestModel createDataTypeRequestModel)
    {
        var attempt = await _dataTypePresentationFactory.CreateAsync(createDataTypeRequestModel);
        if (!attempt.Success)
        {
            return DataTypeOperationStatusResult(attempt.Status);
        }

        Attempt<IDataType, DataTypeOperationStatus> result = await _dataTypeService.CreateAsync(attempt.Result, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDataTypeController>(controller => nameof(controller.ByKey), result.Result.Key)
            : DataTypeOperationStatusResult(result.Status);
    }
}
