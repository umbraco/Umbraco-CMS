using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.RecycleBin;

public class RootMediaRecycleBinController : MediaRecycleBinControllerBase
{
    public RootMediaRecycleBinController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedResult<RecycleBinItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RecycleBinItemViewModel>>> Root(long pageNumber = 0, int pageSize = 100)
        => await GetRoot(pageNumber, pageSize);
}
