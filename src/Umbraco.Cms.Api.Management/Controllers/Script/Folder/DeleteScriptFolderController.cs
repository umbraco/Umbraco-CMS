using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[ApiVersion("1.0")]
public class DeleteScriptFolderController : ScriptFolderControllerBase
{
    private readonly IScriptFolderService _scriptFolderService;

    public DeleteScriptFolderController(IScriptFolderService scriptFolderService)
        => _scriptFolderService = scriptFolderService;

    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptFolderOperationStatus result = await _scriptFolderService.DeleteAsync(path);
        return result is ScriptFolderOperationStatus.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
