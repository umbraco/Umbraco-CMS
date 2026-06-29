using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

/// <summary>
/// Controller for deleting media type folders.
/// </summary>
[ApiVersion("1.0")]
public class DeleteMediaTypeFolderController : MediaTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.Folder.DeleteMediaTypeFolderController"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">An accessor for back office security, used to authorize access to the controller's actions.</param>
    /// <param name="mediaTypeContainerService">A service for managing media type containers.</param>
    public DeleteMediaTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }

    /// <summary>
    /// Deletes a media type folder identified by the provided Id.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
    /// <param name="id">The unique identifier of the media type folder to delete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the delete operation.</returns>
    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a media type folder.")]
    [EndpointDescription("Deletes a media type folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
        => await DeleteFolderAsync(id);
}
