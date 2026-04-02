using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Script.Item;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Item;

/// <summary>
/// Provides API endpoints for managing individual script items within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class ItemScriptItemController : ScriptItemControllerBase
{
    private readonly IFileItemPresentationFactory _fileItemPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemScriptItemController"/> class.
    /// </summary>
    /// <param name="fileItemPresentationFactory">The file item presentation factory.</param>
    public ItemScriptItemController(IFileItemPresentationFactory fileItemPresentationFactory)
        => _fileItemPresentationFactory = fileItemPresentationFactory;

    /// <summary>
    /// Retrieves a collection of script items corresponding to the specified virtual paths.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="paths">A set of virtual paths identifying the script items to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="ScriptItemResponseModel"/> objects representing the requested script items.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ScriptItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of script items.")]
    [EndpointDescription("Gets a collection of script items identified by the provided Ids.")]
    public Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "path")] HashSet<string> paths)
    {
        if (paths.Count is 0)
        {
            return Task.FromResult<IActionResult>(Ok(Enumerable.Empty<ScriptItemResponseModel>()));
        }

        paths = paths.Select(path => path.VirtualPathToSystemPath()).ToHashSet();
        IEnumerable<ScriptItemResponseModel> responseModels = _fileItemPresentationFactory.CreateScriptItemResponseModels(paths);
        return Task.FromResult<IActionResult>(Ok(responseModels));
    }
}
