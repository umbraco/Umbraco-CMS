using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.Tree;

public class MediaTreeItemsController : MediaTreeControllerBase
{
    public MediaTreeItemsController(
        IEntityService entityService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<TreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items([FromQuery(Name = "key")] Guid[] keys)
        => await GetItems(keys);
}
