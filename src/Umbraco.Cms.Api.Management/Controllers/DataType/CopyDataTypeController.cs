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
/// API controller responsible for handling requests to copy data types within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDataTypes)]
public class CopyDataTypeController : DataTypeControllerBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

/// <summary>
/// Initializes a new instance of the <see cref="CopyDataTypeController"/> class.
/// </summary>
/// <param name="dataTypeService">An instance of <see cref="IDataTypeService"/> used to manage data types.</param>
/// <param name="backOfficeSecurityAccessor">An instance of <see cref="IBackOfficeSecurityAccessor"/> used to access back office security information.</param>
    public CopyDataTypeController(IDataTypeService dataTypeService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _dataTypeService = dataTypeService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a copy of the specified data type.
    /// The new data type will have a unique Id and its name will have " (copy)" appended.
    /// Optionally, the copy can be placed in a specified container if a target container Id is provided.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the data type to copy.</param>
    /// <param name="copyDataTypeRequestModel">The request model containing copy options, such as the target container Id.</param>
    /// <returns>A result indicating the outcome of the copy operation.</returns>
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
