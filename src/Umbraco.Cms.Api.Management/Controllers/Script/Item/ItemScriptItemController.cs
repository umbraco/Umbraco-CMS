using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

public class ItemScriptItemController : ScriptItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _fileItemPresentationModelFactory;

    public ItemScriptItemController(FileSystems fileSystems, IFileItemPresentationModelFactory fileItemPresentationModelFactory) => _fileItemPresentationModelFactory = fileItemPresentationModelFactory;

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ScriptItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] string[] paths)
    {
        IEnumerable<ScriptItemResponseModel> reponseModels = _fileItemPresentationModelFactory.CreateScriptItemResponseModels(paths);
        return Ok(reponseModels);
    }
}
