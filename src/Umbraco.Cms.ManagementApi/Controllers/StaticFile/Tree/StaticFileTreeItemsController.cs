using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.StaticFile.Tree;

public class StaticFileTreeItemsController : StaticFileTreeControllerBase
{
    public StaticFileTreeItemsController(IPhysicalFileSystem physicalFileSystem)
        : base(physicalFileSystem)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FileSystemTreeItemViewModel>>> Items([FromQuery(Name = "path")] string[] paths)
        => await GetItems(paths);
}
