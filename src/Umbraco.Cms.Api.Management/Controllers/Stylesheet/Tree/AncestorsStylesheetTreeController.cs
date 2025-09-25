using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[ApiVersion("1.0")]
public class AncestorsStylesheetTreeController : StylesheetTreeControllerBase
{
    private readonly IStyleSheetTreeService _styleSheetTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public AncestorsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : this(styleSheetTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _styleSheetTreeService = styleSheetTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems) =>
        _styleSheetTreeService = styleSheetTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsStylesheetTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IStyleSheetTreeService>(), fileSystems)
    {
    }

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSystemTreeItemPresentationModel>>> Ancestors(
        CancellationToken cancellationToken,
        string descendantPath)
        => await GetAncestors(descendantPath);
}
