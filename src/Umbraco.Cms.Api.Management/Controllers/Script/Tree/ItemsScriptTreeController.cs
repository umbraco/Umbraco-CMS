using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

public class ItemsScriptTreeController : ScriptTreeControllerBase
{
    public ItemsScriptTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemViewModel>>> Items([FromQuery(Name = "path")] string[] paths)
        => await GetItems(paths);
}
