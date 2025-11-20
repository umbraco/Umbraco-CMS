using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

public class SiblingsPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    [ActivatorUtilitiesConstructor]
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public SiblingsPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view tree sibling items.")]
    [EndpointDescription("Gets a collection of partial view tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<FileSystemTreeItemPresentationModel>>> Siblings(
        CancellationToken cancellationToken,
        string path,
        int before,
        int after)
        => await GetSiblings(path, before, after);
}
