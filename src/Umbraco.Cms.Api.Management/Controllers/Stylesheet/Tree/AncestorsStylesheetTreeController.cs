using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// API controller responsible for retrieving ancestor nodes in the stylesheet tree structure within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsStylesheetTreeController : StylesheetTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">
    /// The service used to manage and retrieve stylesheet tree data.
    /// </param>
    public AncestorsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    /// <summary>
    /// Retrieves a collection of stylesheet items that are ancestors of the specified descendant path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantPath">The path of the descendant stylesheet item whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="FileSystemTreeItemPresentationModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor stylesheet items.")]
    [EndpointDescription("Gets a collection of stylesheet items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
