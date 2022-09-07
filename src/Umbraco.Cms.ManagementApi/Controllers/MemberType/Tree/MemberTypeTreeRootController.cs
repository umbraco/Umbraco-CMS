using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.MemberType.Tree;

public class MemberTypeTreeRootController : MemberTypeTreeControllerBase
{
    public MemberTypeTreeRootController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TreeItemViewModel>>> Root(long pageNumber = 0, int pageSize = 100)
        => await GetRoot(pageNumber, pageSize);
}
