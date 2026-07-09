using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// Provides the API root tree controller responsible for managing partial views within the Umbraco CMS management interface.
/// </summary>
[ApiVersion("1.0")]
public class RootPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="RootPartialViewTreeController"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">An instance of <see cref="IPartialViewTreeService"/> used to manage the partial view tree.</param>
    [ActivatorUtilitiesConstructor]
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPartialViewTreeController"/> class, which manages the root tree structure for partial views.
    /// </summary>
    /// <param name="partialViewTreeService">Service used to manage and retrieve partial view tree data.</param>
    /// <param name="fileSystems">Provides access to the file systems required for partial view operations.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootPartialViewTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The file systems used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Gets a paginated collection of partial view items from the root of the tree.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="skip">The number of items to skip for pagination.</param>
    /// <param name="take">The number of items to take for pagination.</param>
    /// <returns>A paged view model containing partial view items from the root of the tree.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of partial view items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
