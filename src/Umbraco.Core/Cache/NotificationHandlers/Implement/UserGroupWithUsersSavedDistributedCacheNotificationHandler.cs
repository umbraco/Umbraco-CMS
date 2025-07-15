using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class UserGroupWithUsersSavedDistributedCacheNotificationHandler : SavedDistributedCacheNotificationHandlerBase<IUserGroup, UserGroupWithUsersSavedNotification, UserGroupWithUsers>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupWithUsersSavedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public UserGroupWithUsersSavedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override IEnumerable<IUserGroup> GetEntities(UserGroupWithUsersSavedNotification notification)
        => notification.SavedEntities.Select(x => x.UserGroup);

    /// <inheritdoc />
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    protected override void Handle(IEnumerable<IUserGroup> entities)
        => Handle(entities, new Dictionary<string, object?>());

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IUserGroup> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshUserGroupCache(entities);
}
