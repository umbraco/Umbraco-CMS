using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.Template.Tree;

public class TemplateTreeChildrenController : TemplateTreeControllerBase
{
    public TemplateTreeChildrenController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<TreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TreeItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100)
        => await GetChildren(parentKey, pageNumber, pageSize);
}
