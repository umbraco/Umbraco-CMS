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
    private readonly IFileItemPresentationModelFactory _presentationModelFactory;

    public ItemStaticFileItemController(IFileItemPresentationModelFactory presentationModelFactory)
        => _presentationModelFactory = presentationModelFactory;

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StaticFileItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] HashSet<string> paths)
    {
        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<StaticFileItemResponseModel> responseModels = _presentationModelFactory.CreateStaticFileItemResponseModels(paths);
        return await Task.FromResult(Ok(responseModels));
    }
}
