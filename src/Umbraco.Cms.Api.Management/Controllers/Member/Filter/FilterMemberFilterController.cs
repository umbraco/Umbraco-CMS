using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Filter;

[ApiVersion("1.0")]
public class FilterMemberFilterController : MemberFilterControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public FilterMemberFilterController(
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory)
    {
        _memberService = memberService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Filter(
        Guid? memberTypeId = null,
        string? memberGroupName = null,
        bool? isApproved = null,
        bool? isLockedOut = null,
        string orderBy = "username",
        Direction orderDirection = Direction.Ascending,
        string? filter = null,
        int skip = 0,
        int take = 100)
    {
        var memberFilter = new MemberFilter()
        {
            MemberTypeId = memberTypeId,
            MemberGroupName = memberGroupName,
            IsApproved = isApproved,
            IsLockedOut = isLockedOut,
            Filter = filter,
        };

        PagedModel<IMember> members = await _memberService.FilterAsync(memberFilter, orderBy, orderDirection, skip, take);

        var pageViewModel = new PagedViewModel<MemberResponseModel>
        {
            Items = await _memberPresentationFactory.CreateMultipleAsync(members.Items),
            Total = members.Total,
        };

        return Ok(pageViewModel);
    }
}
