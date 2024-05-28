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
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    public ItemPartialViewItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<PartialViewItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<PartialViewItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<PartialViewItemResponseModel> responseModels = _fileItemPresentationFactory.CreatePartialViewItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
