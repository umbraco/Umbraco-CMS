using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.StaticFile.Item;

namespace Umbraco.Cms.Api.Management.Controllers.StaticFile.Item;

/// <summary>
/// Provides API endpoints for managing individual static file items in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemStaticFileItemController : StaticFileItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemStaticFileItemController"/> class.
    /// </summary>
    /// <param name="fileItemPresentationFactory">Factory for creating file item presentation models.</param>
    public ItemStaticFileItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    /// <summary>
    /// Retrieves a collection of static file items corresponding to the specified paths.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="paths">A set of virtual paths identifying the static file items to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a collection of <see cref="StaticFileItemResponseModel"/>.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<StaticFileItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of static file items.")]
    [EndpointDescription("Gets a collection of static file items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<StaticFileItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<StaticFileItemResponseModel> responseModels = _fileItemPresentationFactory.CreateStaticFileItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
