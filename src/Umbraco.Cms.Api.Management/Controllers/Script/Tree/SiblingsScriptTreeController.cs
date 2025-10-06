using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

public class SiblingsScriptTreeController : ScriptTreeControllerBase
{
    private readonly IScriptTreeService _scriptTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public SiblingsScriptTreeController(IScriptTreeService scriptTreeService)
        : this(scriptTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _scriptTreeService = scriptTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems) =>
        _scriptTreeService = scriptTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsScriptTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IScriptTreeService>(), fileSystems)
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
