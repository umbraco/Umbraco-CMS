using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Document.Tree;

public class ChildrenDocumentTreeController : DocumentTreeControllerBase
{
    public ChildrenDocumentTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, publicAccessService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedResult<DocumentTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DocumentTreeItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100, string? culture = null)
        => await GetChildren(parentKey, pageNumber, pageSize, culture);
}
