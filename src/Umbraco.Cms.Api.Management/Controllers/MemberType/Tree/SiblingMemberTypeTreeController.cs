using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

public class SiblingMemberTypeTreeController : MemberTypeTreeControllerBase
{
    public SiblingMemberTypeTreeController(
        IEntityService entityService,
        SignProviderCollection signProviders,
        IMemberTypeService memberTypeService)
        : base(entityService, signProviders, memberTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<MemberTypeTreeItemResponseModel>>> GetSiblingMemberTypes(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after)
        => await GetSiblings(target, before, after);
}
