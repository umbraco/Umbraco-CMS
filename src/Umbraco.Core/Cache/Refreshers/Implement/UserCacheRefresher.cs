using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for user caches.
/// </summary>
public sealed class UserCacheRefresher : PayloadCacheRefresherBase<UserCacheRefresherNotification, UserCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public UserCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("E057AF6D-2EE6-41F4-8045-3694010F0AA6");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "User Cache Refresher";

    /// <summary>
    ///     Represents the JSON payload for user cache refresh operations.
    /// </summary>
    public record JsonPayload
    {
        /// <summary>
        ///     Gets the identifier of the user.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        ///     Gets the unique key of the user.
        /// </summary>
        public Guid Key { get; init; }
    }

    /// <inheritdoc />
    public override void RefreshAll()
    {
        ClearAllIsolatedCacheByEntityType<IUser>();
        base.RefreshAll();
    }

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        ClearCache(payloads);
        base.RefreshInternal(payloads);
    }

    private void ClearCache(params JsonPayload[] payloads)
    {
        foreach (JsonPayload p in payloads)
        {
            Attempt<IAppPolicyCache?> userCache = AppCaches.IsolatedCaches.Get<IUser>();
            if (!userCache.Success)
            {
                continue;
            }

            userCache.Result?.Clear(RepositoryCacheKeys.GetKey<IUser, Guid>(p.Key));
            userCache.Result?.Clear(RepositoryCacheKeys.GetKey<IUser, int>(p.Id));
            userCache.Result?.ClearByKey(CacheKeys.UserContentStartNodePathsPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserMediaStartNodePathsPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserAllContentStartNodesPrefix + p.Key);
            userCache.Result?.ClearByKey(CacheKeys.UserAllMediaStartNodesPrefix + p.Key);
        }
    }
}
