using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// Controller responsible for handling operations related to the child nodes within the Stylesheet tree in the management API.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenStylesheetTreeController : StylesheetTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree.ChildrenStylesheetTreeController"/> class with the specified stylesheet tree service.
    /// </summary>
    /// <param name="styleSheetTreeService">The service used to manage and retrieve stylesheet tree data.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ChildrenStylesheetTreeController"/> class.</summary>
    /// <param name="styleSheetTreeService">The stylesheet tree service.</param>
    /// <param name="fileSystems">The file systems.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance that provides access to the file systems required by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public ChildrenStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of stylesheet tree items that are children of the specified parent path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentPath">The path identifying the parent stylesheet tree item.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="PagedViewModel{T}"/> of <see cref="FileSystemTreeItemPresentationModel"/> representing the child stylesheet tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of stylesheet tree child items.")]
    [EndpointDescription("Gets a paginated collection of stylesheet tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Children(
        CancellationToken cancellationToken,
        string parentPath,
        int skip = 0,
        int take = 100)
        => await GetChildren(parentPath, skip, take);
}
