using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.Services.Entities;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Media.Tree;

public class MediaTreeChildrenController : MediaTreeControllerBase
{
    public MediaTreeChildrenController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        : base(entityService, userStartNodeEntitiesService, appCaches, backofficeSecurityAccessor)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<ContentTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ContentTreeItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100)
        => await GetChildren(parentKey, pageNumber, pageSize);
}
