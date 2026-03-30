using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

/// <summary>
/// Provides API endpoints for managing the root of the static file tree in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class RootStaticFileTreeController : StaticFileTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootStaticFileTreeController"/> class.
    /// </summary>
    /// <param name="physicalFileSystem">The <see cref="IPhysicalFileSystem"/> instance representing the physical file system.</param>
    /// <param name="fileSystemTreeService">The <see cref="IPhysicalFileSystemTreeService"/> used to manage the file system tree.</param>
    public RootStaticFileTreeController(IPhysicalFileSystem physicalFileSystem, IPhysicalFileSystemTreeService fileSystemTreeService)
    : base(physicalFileSystem, fileSystemTreeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of static file items from the root of the tree, with optional filtering.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{FileSystemTreeItemPresentationModel}"/> representing the static file items.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of static file items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of static file items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
