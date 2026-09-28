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
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenScriptTreeController"/> class, which handles API requests related to child script tree nodes.
    /// </summary>
    /// <param name="scriptTreeService">The service used to manage and retrieve script tree data.</param>
    public ChildrenScriptTreeController(IScriptTreeService scriptTreeService)
        : base(scriptTreeService)
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
