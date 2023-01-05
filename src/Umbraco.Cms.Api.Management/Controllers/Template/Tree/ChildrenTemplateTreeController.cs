using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.Template.Tree;

public class ChildrenTemplateTreeController : TemplateTreeControllerBase
{
    public ChildrenTemplateTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemViewModel>>> Children(Guid parentKey, int skip = 0, int take = 100)
        => await GetChildren(parentKey, skip, take);
}
