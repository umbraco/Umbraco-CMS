// Copyright (c) Umbraco.
// See LICENSE for more details.

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
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
    private readonly IMemberFilterService _memberFilterService;
    private readonly IMemberPresentationFactory _memberPresentationFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMemberFilterController"/> class.
    /// </summary>
    /// <param name="memberService">Service used for member management operations (unused, retained for DI compatibility).</param>
    /// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="memberFilterService">Service for combined member filtering across content and external stores.</param>
    // TODO (V19): Remove unused parameters which are only here to avoid ambiguous constructor errors.
    [ActivatorUtilitiesConstructor]
    public FilterMemberFilterController(
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMemberFilterService memberFilterService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _memberFilterService = memberFilterService;
        _memberPresentationFactory = memberPresentationFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMemberFilterController"/> class.
    /// </summary>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public FilterMemberFilterController(
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            memberService,
            memberPresentationFactory,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IMemberFilterService>())
    {
    }

    /// <summary>
    /// Retrieves a paged, filtered collection of members based on the specified criteria.
    /// Returns both content-based and external-only members in a unified, correctly paginated result.
    /// </summary>
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
        IUser currentUser = CurrentUser(_backOfficeSecurityAccessor);

        // Approval and lockout state are only disclosed to users with access to sensitive data. Filtering by
        // them would disclose the state of each matched member regardless of the values in the response, so
        // the filter is refused rather than silently widened.
        if ((isApproved is not null || isLockedOut is not null) && currentUser.HasAccessToSensitiveData() is false)
        {
            return Forbidden();
        }

        var memberFilter = new MemberFilter
        {
            MemberTypeId = memberTypeId,
            MemberGroupName = memberGroupName,
            IsApproved = isApproved,
            IsLockedOut = isLockedOut,
            Filter = filter,
        };

        PagedModel<MemberFilterItem> result = await _memberFilterService.FilterAsync(memberFilter, orderBy, orderDirection, skip, take);

        var responseModels = result.Items
            .Select(item => _memberPresentationFactory.CreateFilterItemResponseModel(item, currentUser))
            .ToList();

        return Ok(new PagedViewModel<MemberResponseModel>
        {
            Items = responseModels,
            Total = result.Total,
        });
    }
}
