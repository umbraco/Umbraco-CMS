using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Tree;

/// <summary>
/// Controller responsible for handling API requests related to the ancestor nodes of static files in the file tree.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsStaticFileTreeController : StaticFileTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsStaticFileTreeController"/> class.
    /// </summary>
    /// <param name="physicalFileSystem">The physical file system used by this controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsStaticFileTreeController(IPhysicalFileSystem physicalFileSystem)
        : base(physicalFileSystem)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsStaticFileTreeController"/> class.
    /// </summary>
    /// <param name="physicalFileSystem">The <see cref="IPhysicalFileSystem"/> instance used for file operations.</param>
    /// <param name="fileSystemTreeService">The <see cref="IPhysicalFileSystemTreeService"/> used to manage the file system tree structure.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsStaticFileTreeController(IPhysicalFileSystem physicalFileSystem, IPhysicalFileSystemTreeService fileSystemTreeService)
        : base(physicalFileSystem, fileSystemTreeService)
    {
    }

    /// <summary>
    /// Retrieves a collection of static file items that are ancestors of the specified descendant path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantPath">The path of the static file item whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="FileSystemTreeItemPresentationModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor static file items.")]
    [EndpointDescription("Gets a collection of static file items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
