using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Handles User group cache invalidation/refreshing
/// </summary>
/// <remarks>
///     This also needs to clear the user cache since IReadOnlyUserGroup's are attached to IUser objects
/// </remarks>
public sealed class UserGroupCacheRefresher : CacheRefresherBase<UserGroupCacheRefresherNotification>
{
    #region Define

    public static readonly Guid UniqueId = Guid.Parse("45178038-B232-4FE8-AA1A-F2B949C44762");

    public UserGroupCacheRefresher(AppCaches appCaches, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "User Group Cache Refresher";

    #endregion

    #region Refresher

    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<IUserGroup>();
        Attempt<IAppPolicyCache?> userGroupCache = AppCaches.IsolatedCaches.Get<IUserGroup>();
        if (userGroupCache.Success)
        {
            userGroupCache.Result?.ClearByKey(CacheKeys.UserGroupGetByAliasCacheKeyPrefix);
        }

        // We'll need to clear all user cache too
        ClearAllIsolatedCacheByEntityType<IUser>();

        base.RefreshAll();
    }

    public override void Refresh(int id)
    {
        Remove(id);
        base.Refresh(id);
    }

    public override void Remove(int id)
    {
        Attempt<IAppPolicyCache?> userGroupCache = AppCaches.IsolatedCaches.Get<IUserGroup>();
        if (userGroupCache.Success)
        {
            userGroupCache.Result?.Clear(RepositoryCacheKeys.GetKey<IUserGroup, int>(id));
            userGroupCache.Result?.ClearByKey(CacheKeys.UserGroupGetByAliasCacheKeyPrefix);
        }

        // we don't know what user's belong to this group without doing a look up so we'll need to just clear them all
        ClearAllIsolatedCacheByEntityType<IUser>();

        base.Remove(id);
    }

    #endregion
}
