using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// API controller responsible for managing operations related to the siblings of a stylesheet in the tree structure.
/// </summary>
public class SiblingsStylesheetTreeController : StylesheetTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">
    /// The service used to manage and retrieve stylesheet tree data.
    /// </param>
    public SiblingsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    /// <summary>
    /// Retrieves sibling items of a specified stylesheet tree item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The path of the stylesheet tree item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the specified path.</param>
    /// <param name="after">The number of sibling items to include after the specified path.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="FileSystemTreeItemPresentationModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of stylesheet tree sibling items.")]
    [EndpointDescription("Gets a collection of stylesheet tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
