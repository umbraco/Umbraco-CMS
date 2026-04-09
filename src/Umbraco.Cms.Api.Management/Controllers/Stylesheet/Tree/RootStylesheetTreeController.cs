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
/// Provides the root tree controller for managing stylesheets in the Umbraco CMS API.
/// </summary>
[ApiVersion("1.0")]
public class RootStylesheetTreeController : StylesheetTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    /// <summary>
    /// Initializes a new instance of the <see cref="RootStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">
    /// An instance of <see cref="IStyleSheetTreeService"/> used to manage and retrieve stylesheet tree data.
    /// </param>
    [ActivatorUtilitiesConstructor]
    public RootStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : base(styleSheetTreeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree.RootStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="styleSheetTreeService">Service for managing and retrieving stylesheet tree data.</param>
    /// <param name="fileSystems">Provides access to the file systems used for stylesheet storage and retrieval.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootStylesheetTreeController"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance that provides access to the file systems used by the controller.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootStylesheetTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of stylesheet items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of stylesheet items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
