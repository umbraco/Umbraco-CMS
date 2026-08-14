// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
///     Provides filtered, paginated member listings spanning both content-based and external-only members.
/// </summary>
internal sealed class MemberFilterService : IMemberFilterService
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IMemberFilterRepository _repository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberFilterService"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for unit of work operations.</param>
    /// <param name="repository">The repository for combined member queries.</param>
    public MemberFilterService(ICoreScopeProvider scopeProvider, IMemberFilterRepository repository)
    {
        _scopeProvider = scopeProvider;
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<PagedModel<MemberFilterItem>> FilterAsync(
        MemberFilter filter,
        string orderBy = "username",
        Direction orderDirection = Direction.Ascending,
        int skip = 0,
        int take = 100)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return await _repository.GetPagedByFilterAsync(filter, skip, take, Ordering.By(orderBy, orderDirection));
    }
}
