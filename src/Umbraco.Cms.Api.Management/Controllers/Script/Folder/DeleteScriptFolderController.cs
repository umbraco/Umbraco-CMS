using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

[ApiVersion("1.0")]
public class DeleteScriptFolderController : ScriptFolderControllerBase
{
    public DeleteScriptFolderController(IUmbracoMapper mapper, IScriptFolderService scriptFolderService)
        : base(mapper, scriptFolderService)
    {
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Delete(string path) => DeleteAsync(path);
}
