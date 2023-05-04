using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.RecycleBin;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class RootMediaRecycleBinController : MediaRecycleBinControllerBase
{
    public RootMediaRecycleBinController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RecycleBinItemResponseModel>>> Root(int skip = 0, int take = 100)
        => await GetRoot(skip, take);
}
