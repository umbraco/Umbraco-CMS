using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.PartialView.Tree;

public class ItemsPartialViewTreeController : PartialViewTreeControllerBase
{
    public ItemsPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemViewModel>>> Items([FromQuery(Name = "path")] string[] paths)
        => await GetItems(paths);
}
