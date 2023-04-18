using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Folder;

public class DeleteScriptFolderController : ScriptFolderBaseController
{
    public DeleteScriptFolderController(IUmbracoMapper mapper, IScriptFolderService scriptFolderService) : base(mapper, scriptFolderService)
    {
    }

    [HttpDelete]
    [MapToApiVersion("1.0")]
    public Task<IActionResult> Delete(string path) => DeleteAsync(path);
}
