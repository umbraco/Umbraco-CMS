using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[ApiVersion("1.0")]
public class RootMemberTypeTreeController : MemberTypeTreeControllerBase
{
    public RootMemberTypeTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<EntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<EntityTreeItemResponseModel>>> Root(int skip = 0, int take = 100)
        => await GetRoot(skip, take);
}
