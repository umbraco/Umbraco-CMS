using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

/// <summary>
/// Controller responsible for handling API requests related to retrieving the ancestor nodes of a script within the script tree structure in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsScriptTreeController : ScriptTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsScriptTreeController"/> class, which handles API requests related to ancestor script trees.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree data.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsScriptTreeController"/> class, which handles operations related to ancestor script trees.
    /// </summary>
    /// <param name="scriptTreeService">Service used to manage and retrieve script tree structures.</param>
    /// <param name="fileSystems">Provides access to the file systems used for script storage and retrieval.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsScriptTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to access script file systems.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsScriptTreeController(FileSystems fileSystems)
        : base(fileSystems)
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
