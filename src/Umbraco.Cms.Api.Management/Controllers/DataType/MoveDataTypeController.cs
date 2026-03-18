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

/// <summary>
/// Controller responsible for moving data types within the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class MoveDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDataTypeController"/> class.
    /// </summary>
    /// <param name="dataTypeService">Service used to manage data types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public MoveDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Moves an existing data type identified by the specified <paramref name="id"/> to a different container.
    /// The target container Id must be provided in the <paramref name="moveDataTypeRequestModel"/>.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the data type to move.</param>
    /// <param name="moveDataTypeRequestModel">The request model containing the target container information.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the move operation.</returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a data type.")]
    [EndpointDescription("Moves an existing data type identified by Id to a different container. The target container Id must be provided in the request model.")]
    public async Task<IActionResult> Move(CancellationToken cancellationToken, Guid id, MoveDataTypeRequestModel moveDataTypeRequestModel)
    {
        IDataType? source = await _dataTypeService.GetAsync(id);
        if (source is null)
        {
            return DataTypeNotFound();
        }

        Attempt<IDataType, DataTypeOperationStatus> result = await _dataTypeService.MoveAsync(source, moveDataTypeRequestModel.Target?.Id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DataTypeOperationStatusResult(result.Status);
    }
}
