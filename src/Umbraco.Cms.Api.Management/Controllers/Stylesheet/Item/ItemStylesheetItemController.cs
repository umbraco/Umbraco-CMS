using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Item;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Item;

/// <summary>
/// Provides API endpoints for managing individual stylesheet items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemStylesheetItemController : StylesheetItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the ItemStylesheetItemController class.
    /// </summary>
    /// <param name="fileItemPresentationFactory">Factory for creating file item presentation models.</param>
    public ItemStylesheetItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    /// <summary>
    /// Retrieves a collection of stylesheet items corresponding to the specified virtual paths.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="paths">A set of virtual paths that identify the stylesheet items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="StylesheetItemResponseModel"/> objects representing the requested stylesheet items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StylesheetItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of stylesheet items.")]
    [EndpointDescription("Gets a collection of stylesheet items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<StylesheetItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<StylesheetItemResponseModel> responseModels = _fileItemPresentationFactory.CreateStylesheetItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
