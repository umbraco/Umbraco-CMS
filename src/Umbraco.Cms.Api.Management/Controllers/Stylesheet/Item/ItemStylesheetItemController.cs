using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

[ApiVersion("1.0")]
public class ItemStylesheetItemController : StylesheetItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    public ItemStylesheetItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StylesheetItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Ok(Enumerable.Empty<StylesheetItemResponseModel>());
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<StylesheetItemResponseModel> responseModels = _fileItemPresentationFactory.CreateStylesheetItemResponseModels(paths);
        return await Task.FromResult(Ok(responseModels));
    }
}
