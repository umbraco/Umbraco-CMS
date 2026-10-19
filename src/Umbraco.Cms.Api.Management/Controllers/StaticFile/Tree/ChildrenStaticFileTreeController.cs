using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

/// <summary>
/// Controller responsible for handling operations related to the child nodes within the static file tree structure.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenStaticFileTreeController : StaticFileTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenStaticFileTreeController"/> class.
    /// </summary>
    /// <param name="fileSystemTreeService">Service for managing and interacting with the physical file system tree structure.</param>
    public ChildrenStaticFileTreeController(IPhysicalFileSystemTreeService fileSystemTreeService)
    : base(fileSystemTreeService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of static file tree child items.")]
    [EndpointDescription("Gets a paginated collection of static file tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Children(
        CancellationToken cancellationToken,
        string parentPath,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentPath, skip, take);
}
