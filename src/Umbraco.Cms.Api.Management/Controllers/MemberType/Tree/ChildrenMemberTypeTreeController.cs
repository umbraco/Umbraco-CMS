using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[ApiVersion("1.0")]
public class ChildrenMemberTypeTreeController : MemberTypeTreeControllerBase
{
    public ChildrenMemberTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IMemberTypeService memberTypeService)
        : base(entityService, flagProviders, memberTypeService)
    { }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MemberTypeTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
