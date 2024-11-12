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

[ApiVersion("1.0")]
public class ByPathScriptFolderController : ScriptFolderControllerBase
{
    private readonly IScriptFolderService _scriptFolderService;
    private readonly IUmbracoMapper _mapper;

    public ByPathScriptFolderController(IScriptFolderService scriptFolderService, IUmbracoMapper mapper)
    {
        _scriptFolderService = scriptFolderService;
        _mapper = mapper;
    }

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ScriptFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        ScriptFolderModel? folder = await _scriptFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<ScriptFolderResponseModel>(folder))
            : OperationStatusResult(ScriptFolderOperationStatus.NotFound);
    }
}
