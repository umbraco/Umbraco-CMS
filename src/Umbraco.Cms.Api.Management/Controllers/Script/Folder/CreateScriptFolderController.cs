using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Script.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[ApiVersion("1.0")]
public class CreateScriptFolderController : ScriptFolderControllerBase
{
    private readonly IScriptFolderService _scriptFolderService;
    private readonly IUmbracoMapper _mapper;

    public CreateScriptFolderController(IScriptFolderService scriptFolderService, IUmbracoMapper mapper)
    {
        _scriptFolderService = scriptFolderService;
        _mapper = mapper;
    }


    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateScriptFolderRequestModel requestModel)
    {
        ScriptFolderCreateModel createModel = _mapper.Map<ScriptFolderCreateModel>(requestModel)!;
        Attempt<ScriptFolderModel?, ScriptFolderOperationStatus> result = await _scriptFolderService.CreateAsync(createModel);

        return result.Success
            ? CreatedAtPath<ByPathScriptFolderController>(controller => nameof(controller.ByPath), result.Result!.Path.SystemPathToVirtualPath())
            : OperationStatusResult(result.Status);
    }
}
