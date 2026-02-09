using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
public class CopyDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public CopyDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost("{id:guid}/copy")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Copies a data type.")]
    [EndpointDescription("Creates a duplicate of an existing data type identified by the provided unique Id. The copied data type will be given a new Id and have ' (copy)' appended to its name. Optionally, the copy can be placed in a specific container by providing a target container Id.")]
    public async Task<IActionResult> Copy(CancellationToken cancellationToken, Guid id, CopyDataTypeRequestModel copyDataTypeRequestModel)
    {
        IDataType? source = await _dataTypeService.GetAsync(id);
        if (source is null)
        {
            return DataTypeNotFound();
        }

        Attempt<IDataType, DataTypeOperationStatus> result = await _dataTypeService.CopyAsync(source, copyDataTypeRequestModel.Target?.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyDataTypeController>(controller => nameof(controller.ByKey), result.Result.Key)
            : DataTypeOperationStatusResult(result.Status);
    }
}
