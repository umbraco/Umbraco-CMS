using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Items;

[ApiVersion("1.0")]
public class ItemPartialViewItemController : PartialViewItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _fileItemPresentationModelFactory;
    private readonly IFileSystem _fileSystem;

    public ItemPartialViewItemController(IFileItemPresentationModelFactory fileItemPresentationModelFactory, FileSystems fileSystems)
    {
        _fileItemPresentationModelFactory = fileItemPresentationModelFactory;
        _fileSystem = fileSystems.PartialViewsFileSystem ?? throw new ArgumentException("Missing partial views file system", nameof(fileSystems));
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PartialViewItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "id")] HashSet<string> paths)
    {
        IEnumerable<PartialViewItemResponseModel> responseModels = _fileItemPresentationModelFactory.CreatePartialViewResponseModels(paths, _fileSystem);
        return Ok(responseModels);
    }
}
