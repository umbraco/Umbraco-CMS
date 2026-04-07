using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType;

/// <summary>
/// API controller responsible for handling requests to delete data types in the system.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class DeleteDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDataTypeController"/> class.
    /// </summary>
    /// <param name="dataTypeService">Service used to manage and delete data types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    public DeleteDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Deletes a data type identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the data type to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a data type.")]
    [EndpointDescription("Deletes a data type identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IDataType?, DataTypeOperationStatus> result = await _dataTypeService.DeleteAsync(id, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : DataTypeOperationStatusResult(result.Status);
    }
}
