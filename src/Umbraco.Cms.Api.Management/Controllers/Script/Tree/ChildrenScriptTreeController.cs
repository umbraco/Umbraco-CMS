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
/// Controller responsible for handling requests related to the child nodes of the script tree in the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenScriptTreeController : ScriptTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenScriptTreeController"/> class, which handles API requests related to child script tree nodes.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree data.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenScriptTreeController"/> class, responsible for managing child script tree nodes.
    /// </summary>
    /// <param name="scriptTreeService">Service used to interact with script tree structures.</param>
    /// <param name="fileSystems">Provides access to the file systems used for script storage.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Script.Tree.ChildrenScriptTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to access and manage script file systems.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenScriptTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of script tree child items.")]
    [EndpointDescription("Gets a paginated collection of script tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Children(
        CancellationToken cancellationToken,
        string parentPath,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentPath, skip, take);
}
