using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

/// <summary>
/// Provides API endpoints for managing script folders in Umbraco, identified by their path.
/// </summary>
[ApiVersion("1.0")]
public class ByPathScriptFolderController : ScriptFolderControllerBase
{
    private readonly IScriptFolderService _scriptFolderService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByPathScriptFolderController"/> class.
    /// </summary>
    /// <param name="scriptFolderService">Service for managing script folders.</param>
    /// <param name="mapper">The Umbraco object mapper.</param>
    public ByPathScriptFolderController(IScriptFolderService scriptFolderService, IUmbracoMapper mapper)
    {
        _scriptFolderService = scriptFolderService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a script folder identified by the specified file path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The virtual or system file path used to locate the script folder.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the <see cref="ScriptFolderResponseModel"/> if the folder is found;
    /// otherwise, a 404 Not Found result.
    /// </returns>
    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ScriptFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a script folder by path.")]
    [EndpointDescription("Gets a script folder identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptFolderModel? folder = await _scriptFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<ScriptFolderResponseModel>(folder))
            : OperationStatusResult(ScriptFolderOperationStatus.NotFound);
    }
}
