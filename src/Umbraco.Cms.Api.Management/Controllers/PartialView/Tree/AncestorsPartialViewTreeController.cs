using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

    /// <summary>
    /// Controller responsible for retrieving and managing the ancestor hierarchy of partial views in the tree structure.
    /// </summary>
[ApiVersion("1.0")]
public class AncestorsPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsPartialViewTreeController"/> class, which manages ancestor nodes in the partial view tree.
    /// </summary>
    /// <param name="partialViewTreeService">
    /// The service used to interact with and retrieve data for the partial view tree.
    /// </param>
    [ActivatorUtilitiesConstructor]
    public AncestorsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsPartialViewTreeController"/> class, which provides API endpoints for retrieving ancestor nodes in the partial view tree.
    /// </summary>
    /// <param name="partialViewTreeService">Service used to manage and retrieve partial view tree data.</param>
    /// <param name="fileSystems">The file systems abstraction used for file operations related to partial views.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsPartialViewTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to access file system resources.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves the ancestor partial view items for the specified descendant partial view path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantPath">The file system path of the descendant partial view whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="FileSystemTreeItemPresentationModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor partial view items.")]
    [EndpointDescription("Gets a collection of partial view items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
