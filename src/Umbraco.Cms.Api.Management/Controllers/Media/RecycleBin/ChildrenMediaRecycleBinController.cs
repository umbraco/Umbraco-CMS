using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class ChildrenMediaRecycleBinController : MediaRecycleBinControllerBase
{
    public ChildrenMediaRecycleBinController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RecycleBinItemResponseModel>>> Children(Guid parentId, int skip = 0, int take = 100)
        => await GetChildren(parentId, skip, take);
}
