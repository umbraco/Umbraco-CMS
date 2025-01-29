using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiVersion("1.0")]
public class DeletePartialViewFolderController : PartialViewFolderControllerBase
{
    private readonly IPartialViewFolderService _partialViewFolderService;

    public DeletePartialViewFolderController(IPartialViewFolderService partialViewFolderService)
        => _partialViewFolderService = partialViewFolderService;

    [HttpDelete("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        CancellationToken cancellationToken,
        string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewFolderOperationStatus result = await _partialViewFolderService.DeleteAsync(path);
        return result is PartialViewFolderOperationStatus.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
