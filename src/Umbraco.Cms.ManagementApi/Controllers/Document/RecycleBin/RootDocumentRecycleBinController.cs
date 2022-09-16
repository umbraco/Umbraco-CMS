using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.RecycleBin;

public class RootDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    public RootDocumentRecycleBinController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<RecycleBinItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<RecycleBinItemViewModel>>> Root(long pageNumber = 0, int pageSize = 100)
        => await GetRoot(pageNumber, pageSize);
}
