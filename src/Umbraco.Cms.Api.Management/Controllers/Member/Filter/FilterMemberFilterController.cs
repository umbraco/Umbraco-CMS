using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Member.Filter;

/// <summary>
/// API controller responsible for handling operations related to filtering members in the Umbraco CMS.
/// Provides endpoints for querying and managing member filters.
/// </summary>
[ApiVersion("1.0")]
public class FilterMemberFilterController : MemberFilterControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMemberFilterController"/> class.
    /// </summary>
    /// <param name="memberService">Service used for member management operations.</param>
    /// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    public FilterMemberFilterController(
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberService = memberService;
        _memberPresentationFactory = memberPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Retrieves a paged, filtered collection of members based on the specified criteria.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="memberTypeId">An optional member type identifier to filter the results.</param>
    /// <param name="memberGroupName">An optional member group name to filter the results.</param>
    /// <param name="isApproved">An optional value to filter by member approval status.</param>
    /// <param name="isLockedOut">An optional value to filter by member lockout status.</param>
    /// <param name="orderBy">The field by which to order the results. The default is <c>"username"</c>.</param>
    /// <param name="orderDirection">The direction in which to order the results. The default is <see cref="Direction.Ascending"/>.</param>
    /// <param name="filter">An optional filter string to search for members.</param>
    /// <param name="skip">The number of items to skip for pagination. The default is 0.</param>
    /// <param name="take">The number of items to return for pagination. The default is 100.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{MemberResponseModel}"/> representing the filtered members.</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MemberResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a filtered collection of members.")]
    [EndpointDescription("Filters members based on the provided criteria with support for pagination.")]
    public async Task<IActionResult> Filter(
        CancellationToken cancellationToken,
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
            Items = await _memberPresentationFactory.CreateMultipleAsync(members.Items, CurrentUser(_backOfficeSecurityAccessor)),
            Total = members.Total,
        };

        return Ok(pageViewModel);
    }
}
