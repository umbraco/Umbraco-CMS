using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.FileSystem;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Tree;

[ApiVersion("1.0")]
public class RootPartialViewTreeController : PartialViewTreeControllerBase
{
    // TODO Remove the static service provider, and replace with base when the other constructors are obsoleted.
    [ActivatorUtilitiesConstructor]
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService)
        : base(partialViewTreeService)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootPartialViewTreeController(IPartialViewTreeService partialViewTreeService, FileSystems fileSystems)
        : base(partialViewTreeService, fileSystems)
    {
    }

    [Obsolete("Please use the constructor taking all parameters. Scheduled to be removed in Umbraco 19.")]
    public RootPartialViewTreeController(FileSystems fileSystems)
        : base(fileSystems)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<FileSystemTreeItemPresentationModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of partial view items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of partial view items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<FileSystemTreeItemPresentationModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
