using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

[ApiVersion("1.0")]
public class RootMemberGroupTreeController : MemberGroupTreeControllerBase
{
    public RootMemberGroupTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<NamedEntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<NamedEntityTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
