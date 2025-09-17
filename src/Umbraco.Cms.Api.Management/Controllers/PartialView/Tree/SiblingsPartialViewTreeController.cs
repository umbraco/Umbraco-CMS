using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

public class SiblingsPartialViewTreeController : PartialViewTreeControllerBase
{
    private readonly IPartialViewTreeService _partialViewTreeService;

    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : this(partialViewTreeService, StaticServiceProvider.Instance.GetRequiredService<FileSystems>())
        => _partialViewTreeService = partialViewTreeService;

    [ActivatorUtilitiesConstructor]
    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems) =>
        _partialViewTreeService = partialViewTreeService;

    [Obsolete("Please use the other constructor. Scheduled to be removed in Umbraco 19")]
    public SiblingsPartialViewTreeController(FileSystems fileSystems)
        : this(StaticServiceProvider.Instance.GetRequiredService<IPartialViewTreeService>(), fileSystems)
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
