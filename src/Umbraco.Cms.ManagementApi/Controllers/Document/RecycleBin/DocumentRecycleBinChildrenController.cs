using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.RecycleBin;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.RecycleBin;

public class DocumentRecycleBinChildrenController : DocumentRecycleBinControllerBase
{
    public DocumentRecycleBinChildrenController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedResult<RecycleBinItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RecycleBinItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100)
        => await GetChildren(parentKey, pageNumber, pageSize);
}
