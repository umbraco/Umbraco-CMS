using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class MemberDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<IMember, MemberDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public MemberDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IMember> entities)
        => _distributedCache.RemoveMemberCache(entities);
}
