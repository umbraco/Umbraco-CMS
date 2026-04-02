using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

/// <summary>
/// Provides API endpoints for retrieving the root node of the script tree structure in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class RootScriptTreeController : ScriptTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="RootScriptTreeController"/> class.
    /// </summary>
    /// <param name="scriptTreeService">An instance of <see cref="IScriptTreeService"/> used to manage script trees.</param>
    [ActivatorUtilitiesConstructor]
    public RootScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootScriptTreeController"/> class, which manages the root script tree in the Umbraco backoffice.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree data.</param>
    /// <param name="fileSystems">The file systems abstraction used for accessing script files.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootScriptTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to access script file systems in the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootScriptTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of script items from the root of the tree, with optional filtering.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (for pagination).</param>
    /// <param name="take">The maximum number of items to return (for pagination).</param>
    /// <returns>A <see cref="PagedViewModel{FileSystemTreeItemPresentationModel}"/> containing script items from the root of the tree.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of script items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of script items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
