using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

/// <summary>
/// Controller responsible for handling requests to create new media type folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class CreateMediaTypeFolderController : MediaTypeFolderControllerBase
{
    public CreateMediaTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }

    /// <summary>
    /// Handles the HTTP POST request to create a new media type folder with the specified details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="createFolderRequestModel">The request model containing the name and parent location for the new folder.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>201 Created</c> if the folder is successfully created.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid.</description></item>
    /// <item><description><c>404 Not Found</c> if the specified parent location does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a media type folder.")]
    [EndpointDescription("Creates a new media type folder with the provided name and parent location.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyMediaTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey)).ConfigureAwait(false);
}
