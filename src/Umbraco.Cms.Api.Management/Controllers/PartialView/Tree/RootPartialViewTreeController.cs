using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[ApiVersion("1.0")]
public class RootPartialViewTreeController : PartialViewTreeControllerBase
{
    private readonly IPartialViewTreeService _partialViewTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : this(partialViewTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _partialViewTreeService = partialViewTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems) =>
        _partialViewTreeService = partialViewTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public RootPartialViewTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IPartialViewTreeService>(), fileSystems)
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
