using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

[ApiVersion("1.0")]
public class ItemStaticFileItemController : StaticFileItemControllerBase
{
    private readonly IFileItemPresentationModelFactory _presentationModelFactory;
    private readonly IPhysicalFileSystem _physicalFileSystem;

    public ItemStaticFileItemController(IFileItemPresentationModelFactory presentationModelFactory, IPhysicalFileSystem physicalFileSystem)
    {
        _presentationModelFactory = presentationModelFactory;
        _physicalFileSystem = physicalFileSystem;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StaticFileItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item([FromQuery(Name = "path")] SortedSet<string> paths)
    {
        IEnumerable<StaticFileItemResponseModel> responseModels = _presentationModelFactory.CreateStaticFileItemResponseModels(paths, _physicalFileSystem);
        return Ok(responseModels);
    }
}
