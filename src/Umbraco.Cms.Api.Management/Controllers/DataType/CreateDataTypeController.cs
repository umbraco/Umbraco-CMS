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
/// API controller responsible for handling requests to create new data types in Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class CreateDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IDataTypePresentationFactory _dataTypePresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDataTypeController"/> class.
    /// </summary>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> used to manage data types.</param>
    /// <param name="dataTypePresentationFactory">The <see cref="IDataTypePresentationFactory"/> used to create data type presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">The <see cref="IBackOfficeSecurityAccessor"/> used to access back office security information.</param>
    public CreateDataTypeController(IDataTypeService dataTypeService, IDataTypePresentationFactory dataTypePresentationFactory, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _dataTypePresentationFactory = dataTypePresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new data type using the configuration provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="createDataTypeRequestModel">The model containing the configuration details for the new data type.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the create operation. Returns <c>201 Created</c> on success, or an appropriate error response on failure.
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new data type.")]
    [EndpointDescription("Creates a new data type with the configuration specified in the request model.")]
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
