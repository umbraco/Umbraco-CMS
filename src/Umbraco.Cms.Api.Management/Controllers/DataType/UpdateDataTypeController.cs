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

    /// <summary>
    /// API controller responsible for handling requests to update data types in the Umbraco CMS management interface.
    /// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class UpdateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private IDataTypePresentationFactory _dataTypePresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDataTypeController"/> class, responsible for handling data type update operations in the management API.
    /// </summary>
    /// <param name="dataTypeService">Service used to manage data types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="dataTypePresentationFactory">Factory for creating data type presentation models.</param>
    public UpdateDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypePresentationFactory dataTypePresentationFactory)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _dataTypePresentationFactory = dataTypePresentationFactory;
    }

    /// <summary>
    /// Updates the data type with the specified ID using the provided request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the data type to update.</param>
    /// <param name="updateDataTypeViewModel">The model containing the updated data type details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a data type.")]
    [EndpointDescription("Updates a data type identified by the provided Id with the details from the request model.")]
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
