using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.Script.Tree;

[ApiVersion("1.0")]
public class AncestorsScriptTreeController : ScriptTreeControllerBase
{
    private readonly IScriptTreeService _scriptTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public AncestorsScriptTreeController(IScriptTreeService scriptTreeService)
        : this(scriptTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _scriptTreeService = scriptTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsScriptTreeController(IScriptTreeService scriptTreeService, FileSystems fileSystems)
        : base(scriptTreeService, fileSystems) =>
        _scriptTreeService = scriptTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsScriptTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IScriptTreeService>(), fileSystems)
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
