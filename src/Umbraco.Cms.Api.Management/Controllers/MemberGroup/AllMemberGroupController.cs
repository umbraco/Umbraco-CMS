using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.MemberGroup;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup;

[ApiVersion("1.0")]
public class AllMemberGroupController : MemberGroupControllerBase
{
    private readonly IMemberGroupService _memberGroupService;
    private readonly IUmbracoMapper _mapper;

    public AllMemberGroupController(IMemberGroupService memberGroupService, IUmbracoMapper mapper)
    {
        _memberGroupService = memberGroupService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberGroupResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<MemberGroupResponseModel>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        IMemberGroup[] memberGroups = (await _memberGroupService.GetAllAsync()).ToArray();
        var viewModel = new PagedViewModel<MemberGroupResponseModel>
        {
            Total = memberGroups.Length,
            Items = _mapper.MapEnumerable<IMemberGroup, MemberGroupResponseModel>(memberGroups.Skip(skip).Take(take)),
        };

        return Ok(viewModel);
    }
}
