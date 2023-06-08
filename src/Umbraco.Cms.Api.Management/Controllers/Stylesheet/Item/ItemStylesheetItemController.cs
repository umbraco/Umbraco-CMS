using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

[ApiVersion("1.0")]
public class ItemStylesheetItemController : StylesheetItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _fileItemPresentationModelFactory;
    private readonly IFileSystem _fileSystem;

    public ItemStylesheetItemController(FileSystems fileSystems, IFileItemPresentationModelFactory fileItemPresentationModelFactory)
    {
        _fileItemPresentationModelFactory = fileItemPresentationModelFactory;
        _fileSystem = fileSystems.StylesheetsFileSystem ?? throw new ArgumentException("Missing stylesheet file system", nameof(fileSystems));
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ScriptItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] HashSet<string> paths)
    {
        IEnumerable<StylesheetItemResponseModel> responseModels = _fileItemPresentationModelFactory.CreateStylesheetItemResponseModels(paths, _fileSystem);
        return Ok(responseModels);
    }
}
