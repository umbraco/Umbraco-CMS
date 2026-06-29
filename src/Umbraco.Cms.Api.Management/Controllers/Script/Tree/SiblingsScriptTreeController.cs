using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

/// <summary>
/// API controller for retrieving and managing sibling script nodes within the script tree structure in Umbraco CMS.
/// Provides endpoints to navigate and interact with scripts that share the same parent node.
/// </summary>
public class SiblingsScriptTreeController : ScriptTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsScriptTreeController"/> class, which provides API endpoints for managing sibling script tree nodes.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree structures.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsScriptTreeController"/> class, which manages sibling script tree operations.
    /// </summary>
    /// <param name="scriptTreeService">Service for handling script tree operations.</param>
    /// <param name="fileSystems">Provides access to the file systems used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsScriptTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance that provides access to the script file systems used by this controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsScriptTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves a collection of script tree items that are siblings of the specified path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The path of the script tree item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the specified path.</param>
    /// <param name="after">The number of sibling items to include after the specified path.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="FileSystemTreeItemPresentationModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of script tree sibling items.")]
    [EndpointDescription("Gets a collection of script tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
