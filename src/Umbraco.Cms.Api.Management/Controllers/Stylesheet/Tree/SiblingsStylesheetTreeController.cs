using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// API controller responsible for managing operations related to the siblings of a stylesheet in the tree structure.
/// </summary>
public class SiblingsStylesheetTreeController : StylesheetTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">
    /// The service used to manage and retrieve stylesheet tree data.
    /// </param>
    [ActivatorUtilitiesConstructor]
    public SiblingsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

/// <summary>
/// Initializes a new instance of the <see cref="SiblingsStylesheetTreeController"/> class.
/// </summary>
/// <param name="styleSheetTreeService">Service for managing stylesheet tree operations.</param>
/// <param name="fileSystems">Provides access to the file systems used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> providing access to the file systems required by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves sibling items of a specified stylesheet tree item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The path of the stylesheet tree item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the specified path.</param>
    /// <param name="after">The number of sibling items to include after the specified path.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="FileSystemTreeItemPresentationModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of stylesheet tree sibling items.")]
    [EndpointDescription("Gets a collection of stylesheet tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
