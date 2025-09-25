using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Tree;

public class SiblingsStylesheetTreeController : StylesheetTreeControllerBase
{
    private readonly IStyleSheetTreeService _styleSheetTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public SiblingsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService)
        : this(styleSheetTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _styleSheetTreeService = styleSheetTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsStylesheetTreeController(IStyleSheetTreeService styleSheetTreeService, FileSystems fileSystems)
        : base(styleSheetTreeService, fileSystems) =>
        _styleSheetTreeService = styleSheetTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsStylesheetTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IStyleSheetTreeService>(), fileSystems)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
