using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

/// <summary>
/// Provides API endpoints for deleting script folders in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class DeleteScriptFolderController : ScriptFolderControllerBase
{
    private readonly IScriptFolderService _scriptFolderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteScriptFolderController"/> class, responsible for handling requests to delete script folders.
    /// </summary>
    /// <param name="scriptFolderService">An instance of <see cref="IScriptFolderService"/> used to manage script folder operations.</param>
    public DeleteScriptFolderController(IScriptFolderService scriptFolderService)
        => _scriptFolderService = scriptFolderService;

    /// <summary>
    /// Deletes the script folder at the specified path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual path of the script folder to delete.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// returns 200 OK if the folder was deleted; 400 Bad Request or 404 Not Found if the operation failed.
    /// </returns>
    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a script folder.")]
    [EndpointDescription("Deletes a script folder identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptFolderOperationStatus result = await _scriptFolderService.DeleteAsync(path);
        return result is ScriptFolderOperationStatus.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
