using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

/// <summary>
/// Controller responsible for handling requests to update media type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class UpdateMediaTypeFolderController : MediaTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.Folder.UpdateMediaTypeFolderController"/> class,
    /// providing dependencies required for managing media type folders in the back office.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">The accessor used to retrieve the current back office security context.</param>
    /// <param name="mediaTypeContainerService">The service responsible for operations on media type containers (folders).</param>
    public UpdateMediaTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }

    /// <summary>Updates a media type folder identified by the provided Id with the details from the request model.</summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media type folder to update.</param>
    /// <param name="updateFolderResponseModel">The model containing updated folder details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a media type folder.")]
    [EndpointDescription("Updates a media type folder identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateFolderResponseModel updateFolderResponseModel)
        => await UpdateFolderAsync(id, updateFolderResponseModel);
}
