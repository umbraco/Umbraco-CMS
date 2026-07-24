// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class ExternalMemberDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<ExternalMemberIdentity, ExternalMemberDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMemberDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public ExternalMemberDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<ExternalMemberIdentity> entities, IDictionary<string, object?> state)
        => _distributedCache.RemoveExternalMemberCache(entities);
}
