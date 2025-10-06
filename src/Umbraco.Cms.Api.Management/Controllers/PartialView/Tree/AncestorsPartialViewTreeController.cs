using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[ApiVersion("1.0")]
public class AncestorsPartialViewTreeController : PartialViewTreeControllerBase
{
    private readonly IPartialViewTreeService _partialViewTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public AncestorsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
    : this(partialViewTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _partialViewTreeService = partialViewTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
    : base(partialViewTreeService, fileSystems) =>
        _partialViewTreeService = partialViewTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public AncestorsPartialViewTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IPartialViewTreeService>(), fileSystems)
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
