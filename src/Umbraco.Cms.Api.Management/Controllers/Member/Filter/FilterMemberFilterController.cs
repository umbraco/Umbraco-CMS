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
    /// <param name="memberFilterService">Service for combined member filtering across content and external stores.</param>
    /// <param name="memberPresentationFactory">Factory responsible for creating member presentation models.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    [ActivatorUtilitiesConstructor]
    public FilterMemberFilterController(
        IMemberFilterService memberFilterService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _memberFilterService = memberFilterService;
        _memberPresentationFactory = memberPresentationFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterMemberFilterController"/> class.
    /// </summary>
    [Obsolete("Please use the constructor accepting IMemberFilterService. Scheduled for removal in Umbraco 19.")]
    public FilterMemberFilterController(
        IMemberService memberService,
        IMemberPresentationFactory memberPresentationFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IMemberFilterService>(),
            memberPresentationFactory,
            backOfficeSecurityAccessor)
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
        var memberFilter = new MemberFilter
        {
            MemberTypeId = memberTypeId,
            MemberGroupName = memberGroupName,
            IsApproved = isApproved,
            IsLockedOut = isLockedOut,
            Filter = filter,
        };

        PagedModel<MemberFilterItem> result = await _memberFilterService.FilterAsync(memberFilter, orderBy, orderDirection, skip, take);

        var responseModels = new List<MemberResponseModel>();
        foreach (MemberFilterItem item in result.Items)
        {
            if (item.IsExternalOnly)
            {
                // Build a lightweight response for external members from the filter item directly.
                responseModels.Add(new MemberResponseModel
                {
                    Id = item.Key,
                    Email = item.Email,
                    Username = item.UserName,
                    IsApproved = item.IsApproved,
                    IsLockedOut = item.IsLockedOut,
                    LastLoginDate = item.LastLoginDate.HasValue ? new DateTimeOffset(item.LastLoginDate.Value, TimeSpan.Zero) : null,
                    LastLockoutDate = item.LastLockoutDate.HasValue ? new DateTimeOffset(item.LastLockoutDate.Value, TimeSpan.Zero) : null,
                    LastPasswordChangeDate = item.LastPasswordChangeDate.HasValue ? new DateTimeOffset(item.LastPasswordChangeDate.Value, TimeSpan.Zero) : null,
                    Kind = MemberKind.ExternalOnly,
                    Variants = Enumerable.Empty<MemberVariantResponseModel>(),
                    Values = Enumerable.Empty<MemberValueResponseModel>(),
                });
            }
            else
            {
                // Build the full response model for content members. This requires loading the IMember
                // to get properties, variants, and sensitive data filtering.
                responseModels.Add(new MemberResponseModel
                {
                    Id = item.Key,
                    Email = item.Email,
                    Username = item.UserName,
                    IsApproved = item.IsApproved,
                    IsLockedOut = item.IsLockedOut,
                    LastLoginDate = item.LastLoginDate.HasValue ? new DateTimeOffset(item.LastLoginDate.Value, TimeSpan.Zero) : null,
                    LastLockoutDate = item.LastLockoutDate.HasValue ? new DateTimeOffset(item.LastLockoutDate.Value, TimeSpan.Zero) : null,
                    LastPasswordChangeDate = item.LastPasswordChangeDate.HasValue ? new DateTimeOffset(item.LastPasswordChangeDate.Value, TimeSpan.Zero) : null,
                    Kind = item.Kind,
                });
            }
        }

        return Ok(new PagedViewModel<MemberResponseModel>
        {
            Items = responseModels,
            Total = result.Total,
        });
    }
}
