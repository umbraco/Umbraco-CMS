using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiVersion("1.0")]
public class DeleteStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;

    public DeleteStylesheetFolderController(IStylesheetFolderService stylesheetFolderService)
        => _stylesheetFolderService = stylesheetFolderService;

    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetFolderOperationStatus result = await _stylesheetFolderService.DeleteAsync(path);
        return result is StylesheetFolderOperationStatus.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
