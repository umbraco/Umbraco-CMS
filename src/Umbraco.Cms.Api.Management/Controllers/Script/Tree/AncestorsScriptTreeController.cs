using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

/// <summary>
/// Controller responsible for handling API requests related to retrieving the ancestor nodes of a script within the script tree structure in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsScriptTreeController : ScriptTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsScriptTreeController"/> class, which handles API requests related to ancestor script trees.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree data.</param>
    public AncestorsScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }

    /// <summary>
    /// Retrieves a collection of script items that are ancestors of the specified descendant script path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantPath">The path identifying the descendant script item whose ancestors are to be retrieved.</param>
    /// <returns>A collection of ancestor script items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor script items.")]
    [EndpointDescription("Gets a collection of script items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
