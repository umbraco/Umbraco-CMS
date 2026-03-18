using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// Controller responsible for handling requests related to the child nodes of the Partial View tree in the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenPartialViewTreeController"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage partial view trees.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenPartialViewTreeController"/> class.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage partial view trees.</param>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> providing access to the file system resources.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenPartialViewTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The file systems used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of partial view tree items that are children of the specified parent path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentPath">The path of the parent item whose children are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a paged view model of <see cref="FileSystemTreeItemPresentationModel"/> items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view tree child items.")]
    [EndpointDescription("Gets a paginated collection of partial view tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Children(
        CancellationToken cancellationToken,
        string parentPath,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentPath, skip, take);
}
