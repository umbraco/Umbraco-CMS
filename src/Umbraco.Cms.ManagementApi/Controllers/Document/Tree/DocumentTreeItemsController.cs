using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.Tree;

public class DocumentTreeItemsController : DocumentTreeControllerBase
{
    public DocumentTreeItemsController(
            IEntityService entityService,
            IPublicAccessService publicAccessService,
            AppCaches appCaches,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, publicAccessService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<DocumentTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Items([FromQuery(Name = "key")] Guid[] keys)
        => await GetItems(keys);
}
