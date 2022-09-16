using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.StaticFile.Tree;

public class ChildrenStaticFileTreeController : StaticFileTreeControllerBase
{
    public ChildrenStaticFileTreeController(IPhysicalFileSystem physicalFileSystem)
        : base(physicalFileSystem)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemViewModel>>> Children(string path, long pageNumber = 0, int pageSize = 100)
        => await GetChildren(path, pageNumber, pageSize);
}
