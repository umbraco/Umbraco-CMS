using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

[ApiVersion("1.0")]
public class ItemStaticFileItemController : StaticFileItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    public ItemStaticFileItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StaticFileItemResponseModel>), StatusCodes.Status200OK)]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<StaticFileItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<StaticFileItemResponseModel> responseModels = _fileItemPresentationFactory.CreateStaticFileItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
