using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

/// <summary>
/// API controller responsible for handling operations related to the siblings tree of partial views in Umbraco.
/// Provides endpoints for retrieving and managing sibling partial views within the CMS.
/// </summary>
public class SiblingsPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsPartialViewTreeController"/> class with the specified partial view tree service.
    /// </summary>
    /// <param name="partialViewTreeService">
    /// The service used to manage and retrieve partial view tree structures.
    /// </param>
    [ActivatorUtilitiesConstructor]
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsPartialViewTreeController"/> class, which manages operations related to sibling partial views in the tree structure.
    /// </summary>
    /// <param name="partialViewTreeService">Service used to interact with and manage partial view trees.</param>
    /// <param name="fileSystems">Provides access to the file systems used for storing partial views.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsPartialViewTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to access and manage file system operations for partial views.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>Gets a collection of partial view tree sibling items.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="path">The path of the partial view to find siblings for.</param>
    /// <param name="before">The number of sibling items to retrieve before the specified path.</param>
    /// <param name="after">The number of sibling items to retrieve after the specified path.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a subset view model of file system tree item presentations.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view tree sibling items.")]
    [EndpointDescription("Gets a collection of partial view tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
