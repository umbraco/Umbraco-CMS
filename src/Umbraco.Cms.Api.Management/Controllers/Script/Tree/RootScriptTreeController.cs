using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[ApiVersion("1.0")]
public class RootScriptTreeController : ScriptTreeControllerBase
{
    private readonly IScriptTreeService _scriptTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public RootScriptTreeController(IScriptTreeService scriptTreeService)
        : this(scriptTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _scriptTreeService = scriptTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems) =>
        _scriptTreeService = scriptTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootScriptTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IScriptTreeService>(), fileSystems)
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
