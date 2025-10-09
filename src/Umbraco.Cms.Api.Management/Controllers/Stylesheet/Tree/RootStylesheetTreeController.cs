using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

[ApiVersion("1.0")]
public class RootStylesheetTreeController : StylesheetTreeControllerBase
{
    private readonly IStyleSheetTreeService _styleSheetTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public RootStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : this(styleSheetTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _styleSheetTreeService = styleSheetTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems) =>
        _styleSheetTreeService = styleSheetTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootStylesheetTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IStyleSheetTreeService>(), fileSystems)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
