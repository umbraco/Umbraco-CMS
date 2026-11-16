using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// API controller responsible for handling operations related to the siblings tree of partial views in Umbraco.
/// Provides endpoints for retrieving and managing sibling partial views within the CMS.
/// </summary>
public class SiblingsPartialViewTreeController : PartialViewTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsPartialViewTreeController"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">
    /// The service used to manage and retrieve partial view tree structures.
    /// </param>
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    /// <summary>Gets a collection of partial view tree sibling items.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="path">The path of the partial view to find siblings for.</param>
    /// <param name="before">The number of sibling items to retrieve before the specified path.</param>
    /// <param name="after">The number of sibling items to retrieve after the specified path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a subset view model of file system tree item presentations.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view tree sibling items.")]
    [EndpointDescription("Gets a collection of partial view tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
