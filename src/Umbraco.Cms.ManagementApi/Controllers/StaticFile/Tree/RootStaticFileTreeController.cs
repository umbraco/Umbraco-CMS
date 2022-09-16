using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.StaticFile.Tree;

public class RootStaticFileTreeController : StaticFileTreeControllerBase
{
    public RootStaticFileTreeController(IPhysicalFileSystem physicalFileSystem)
        : base(physicalFileSystem)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemViewModel>>> Root(long pageNumber = 0, int pageSize = 100)
        => await GetRoot(pageNumber, pageSize);
}
