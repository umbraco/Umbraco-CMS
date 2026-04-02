using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

/// <summary>
/// API controller responsible for handling requests to delete stylesheet folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteStylesheetFolderController"/> class.
    /// </summary>
    /// <param name="stylesheetFolderService">The service used to manage stylesheet folders, injected into the controller.</param>
    public DeleteStylesheetFolderController(IStylesheetFolderService stylesheetFolderService)
        => _stylesheetFolderService = stylesheetFolderService;

    /// <summary>
    /// Deletes the stylesheet folder at the specified path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path of the stylesheet folder to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the delete operation.
    /// Returns <c>200 OK</c> if successful, or a <see cref="ProblemDetails"/> response if the folder is not found or the request is invalid.
    /// </returns>
    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a stylesheet folder.")]
    [EndpointDescription("Deletes a stylesheet folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetFolderOperationStatus result = await _stylesheetFolderService.DeleteAsync(path);
        return result is StylesheetFolderOperationStatus.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
