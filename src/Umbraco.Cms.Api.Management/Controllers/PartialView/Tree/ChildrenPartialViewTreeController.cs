using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// Controller responsible for handling requests related to the child nodes of the Partial View tree in the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenPartialViewTreeController : PartialViewTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenPartialViewTreeController"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage partial view trees.</param>
    public ChildrenPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
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
