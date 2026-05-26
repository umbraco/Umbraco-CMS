// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides combined, paginated member queries across both the content member store
///     and the external member store.
/// </summary>
public interface IMemberFilterRepository
{
    /// <summary>
    ///     Gets a paged, filtered result of members from both the content and external member tables.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to return.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <returns>A paged model of <see cref="MemberFilterItem"/> instances.</returns>
    Task<PagedModel<MemberFilterItem>> GetPagedByFilterAsync(MemberFilter filter, int skip, int take, Ordering ordering);
}
