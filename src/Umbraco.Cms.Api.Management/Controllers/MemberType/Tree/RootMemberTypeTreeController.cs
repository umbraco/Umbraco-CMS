using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Signs;

namespace Umbraco.Cms.Api.Management.Controllers.MemberType.Tree;

[ApiVersion("1.0")]
public class RootMemberTypeTreeController : MemberTypeTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootMemberTypeTreeController(IEntityService entityService, IMemberTypeService memberTypeService)
        : base(entityService, memberTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public RootMemberTypeTreeController(IEntityService entityService, SignProviderCollection signProviders, IMemberTypeService memberTypeService)
        : base(entityService, signProviders, memberTypeService)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MemberTypeTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
