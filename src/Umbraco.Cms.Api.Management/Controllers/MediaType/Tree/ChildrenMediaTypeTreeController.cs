using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Tree;

[ApiVersion("1.0")]
public class ChildrenMediaTypeTreeController : MediaTypeTreeControllerBase
{
    public ChildrenMediaTypeTreeController(IEntityService entityService, IMediaTypeService mediaTypeService)
        : base(entityService, mediaTypeService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MediaTypeTreeItemResponseModel>>> Children(Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
