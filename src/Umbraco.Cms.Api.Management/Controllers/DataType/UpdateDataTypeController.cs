using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.DataType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class UpdateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private IDataTypePresentationFactory _dataTypePresentationFactory;

    public UpdateDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypePresentationFactory dataTypePresentationFactory)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dataTypePresentationFactory = dataTypePresentationFactory;
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateDataTypeRequestModel updateDataTypeViewModel)
    {
        IDataType? current = await _dataTypeService.GetAsync(id);
        if (current == null)
        {
            return DataTypeNotFound();
        }

        Attempt<IDataType, DataTypeOperationStatus> attempt = await _dataTypePresentationFactory.CreateAsync(updateDataTypeViewModel, current);
        if (!attempt.Success)
        {
            return DataTypeOperationStatusResult(attempt.Status);
        }

        Attempt<IDataType, DataTypeOperationStatus> result = await _dataTypeService.UpdateAsync(attempt.Result, CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : DataTypeOperationStatusResult(result.Status);
    }
}
