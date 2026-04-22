using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

/// <summary>
/// Controller for managing media type folders identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyMediaTypeFolderController : MediaTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.MediaType.Folder.ByKeyMediaTypeFolderController"/> class with the specified dependencies.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security information for authorization purposes.</param>
    /// <param name="mediaTypeContainerService">Service for managing media type containers.</param>
    public ByKeyMediaTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }

    /// <summary>
    /// Retrieves a media type folder by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the media type folder to retrieve.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation, containing the media type folder if found; otherwise, a not found result.</returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a media type folder.")]
    [EndpointDescription("Gets a media type folder identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id) => await GetFolderAsync(id);
}
