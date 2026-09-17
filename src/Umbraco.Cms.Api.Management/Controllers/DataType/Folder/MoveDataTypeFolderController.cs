using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

/// <summary>
/// Controller responsible for handling HTTP requests related to moving data type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class MoveDataTypeFolderController : DataTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MoveDataTypeFolderController"/> class, which handles requests for moving data type folders in the Umbraco back office.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="dataTypeContainerService">Service used to manage data type containers (folders) within the system.</param>
    public MoveDataTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    /// <summary>
    /// Moves a data type folder identified by the provided <paramref name="id"/> to a different location.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the data type folder to move.</param>
    /// <param name="moveFolderRequestModel">The request model containing the target location for the move.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the move operation.</returns>
    [HttpPut("{id:guid}/move")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Moves a data type folder.")]
    [EndpointDescription("Moves a data type folder identified by the provided Id to a different location.")]
    public async Task<IActionResult> Move(
        CancellationToken cancellationToken,
        Guid id,
        MoveFolderRequestModel moveFolderRequestModel)
        => await MoveFolderAsync(id, moveFolderRequestModel);
}
