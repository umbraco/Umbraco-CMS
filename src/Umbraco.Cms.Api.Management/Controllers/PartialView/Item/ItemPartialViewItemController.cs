using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Item;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Item;

[ApiVersion("1.0")]
public class ItemPartialViewItemController : PartialViewItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _fileItemPresentationModelFactory;

    public ItemPartialViewItemController(IFileItemPresentationModelFactory fileItemPresentationModelFactory)
        => _fileItemPresentationModelFactory = fileItemPresentationModelFactory;

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PartialViewItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] HashSet<string> paths)
    {
        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<PartialViewItemResponseModel> responseModels = _fileItemPresentationModelFactory.CreatePartialViewItemResponseModels(paths);
        return await Task.FromResult(Ok(responseModels));
    }
}
