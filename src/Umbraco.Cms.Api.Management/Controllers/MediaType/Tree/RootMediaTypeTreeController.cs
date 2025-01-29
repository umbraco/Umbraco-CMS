using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

[ApiVersion("1.0")]
public class RootMediaTypeTreeController : MediaTypeTreeControllerBase
{
    public RootMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MediaTypeTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
