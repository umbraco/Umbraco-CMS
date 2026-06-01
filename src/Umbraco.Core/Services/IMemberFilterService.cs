// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides filtered, paginated member listings that span both content-based and external-only members.
/// </summary>
public interface IMemberFilterService
{
    /// <summary>
    ///     Filters members across both content and external member stores, returning a unified paged result
    ///     ordered and paginated at the database level.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="orderBy">The field to order by (e.g. "username", "email"). Defaults to "username".</param>
    /// <param name="orderDirection">The sort direction.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <returns>A paged model of <see cref="MemberFilterItem"/> instances.</returns>
    Task<PagedModel<MemberFilterItem>> FilterAsync(
        MemberFilter filter,
        string orderBy = "username",
        Direction orderDirection = Direction.Ascending,
        int skip = 0,
        int take = 100);
}
