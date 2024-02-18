using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Member;

[ApiVersion("1.0")]
public class FilterMemberController : MemberControllerBase
{
    private readonly IMemberTypeService _memberTypeService;
    private readonly IMemberService _memberService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;

    public FilterMemberController(
        IMemberTypeService memberTypeService,
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory)
    {
        _memberTypeService = memberTypeService;
        _memberService = memberService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    [HttpGet("filter")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Filter(
        Guid? memberTypeId = null,
        string orderBy = "username",
        Direction orderDirection = Direction.Ascending,
        string? filter = null,
        int skip = 0,
        int take = 100)
    {
        // TODO: Move to service once we have FilterAsync method for members
        string? memberTypeAlias = null;
        if (memberTypeId.HasValue)
        {
            IMemberType? memberType = await _memberTypeService.GetAsync(memberTypeId.Value);
            if (memberType == null)
            {
                return MemberEditingOperationStatusResult(MemberEditingOperationStatus.MemberTypeNotFound);
            }

            memberTypeAlias = memberType.Alias;
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IMember> members = await Task.FromResult(_memberService.GetAll(
            pageNumber,
            pageSize,
            out var totalRecords,
            orderBy,
            orderDirection,
            memberTypeAlias,
            filter ?? string.Empty));

        var pageViewModel = new PagedViewModel<MemberResponseModel>
        {
            Items = await _memberPresentationFactory.CreateMultipleAsync(members),
            Total = totalRecords,
        };

        return Ok(pageViewModel);
    }
}
