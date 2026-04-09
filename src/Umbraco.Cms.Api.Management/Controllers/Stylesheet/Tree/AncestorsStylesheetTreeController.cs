using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

/// <summary>
/// API controller responsible for retrieving ancestor nodes in the stylesheet tree structure within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsStylesheetTreeController : StylesheetTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">
    /// The service used to manage and retrieve stylesheet tree data.
    /// </param>
    [ActivatorUtilitiesConstructor]
    public AncestorsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree.AncestorsStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">Service for managing stylesheet tree operations.</param>
    /// <param name="fileSystems">Provides access to file system abstractions.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsStylesheetTreeController"/> class with the specified file systems.
    /// </summary>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> to be used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public AncestorsStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    /// <summary>
    /// Retrieves a collection of stylesheet items that are ancestors of the specified descendant path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantPath">The path of the descendant stylesheet item whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="FileSystemTreeItemPresentationModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor stylesheet items.")]
    [EndpointDescription("Gets a collection of stylesheet items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
