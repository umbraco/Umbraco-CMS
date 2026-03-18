using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

/// <summary>
/// Controller responsible for handling requests to delete data type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteDataTypeFolderController : DataTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDataTypeFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security operations.</param>
    /// <param name="dataTypeContainerService">Service for managing data type containers (folders).</param>
    public DeleteDataTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    /// <summary>
    /// Deletes a data type folder identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the data type folder to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a data type folder.")]
    [EndpointDescription("Deletes a data type folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id) => await DeleteFolderAsync(id);
}
