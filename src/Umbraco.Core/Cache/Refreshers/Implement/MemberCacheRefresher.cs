using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for member caches.
/// </summary>
public sealed class MemberCacheRefresher : PayloadCacheRefresherBase<MemberCacheRefresherNotification, MemberCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("E285DF34-ACDC-4226-AE32-C0CB5CF388DA");

    private readonly IIdKeyMap _idKeyMap;
    private readonly IMemberPartialViewCacheInvalidator _memberPartialViewCacheInvalidator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 18.")]
    public MemberCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IIdKeyMap idKeyMap, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : this(
            appCaches,
            serializer,
            idKeyMap,
            eventAggregator,
            factory,
            StaticServiceProvider.Instance.GetRequiredService<IMemberPartialViewCacheInvalidator>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="idKeyMap">The ID-key mapping service.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    /// <param name="memberPartialViewCacheInvalidator">The member partial view cache invalidator.</param>
    public MemberCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory,
        IMemberPartialViewCacheInvalidator memberPartialViewCacheInvalidator)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _memberPartialViewCacheInvalidator = memberPartialViewCacheInvalidator;
    }

    #region Indirect

    /// <summary>
    ///     Refreshes member type caches by clearing all cached members.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    public static void RefreshMemberTypes(AppCaches appCaches) => appCaches.IsolatedCaches.ClearCache<IMember>();

    #endregion

    /// <summary>
    ///     Represents the JSON payload for member cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the member.</param>
        /// <param name="username">The username of the member.</param>
        /// <param name="removed">Whether the member was removed.</param>
        public JsonPayload(int id, string? username, bool removed)
            : this(id, username, removed, indexableFieldsChanged: true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the member.</param>
        /// <param name="username">The username of the member.</param>
        /// <param name="removed">Whether the member was removed.</param>
        /// <param name="indexableFieldsChanged">
        ///     Whether any field that is part of the Examine value set has changed as part of this operation.
        ///     When <c>false</c>, Examine indexing handlers will skip the re-index for this payload.
        /// </param>
        public JsonPayload(int id, string? username, bool removed, bool indexableFieldsChanged)
        {
            Id = id;
            Username = username;
            Removed = removed;
            IndexableFieldsChanged = indexableFieldsChanged;
        }

        /// <summary>
        ///     Gets the identifier of the member.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the username of the member.
        /// </summary>
        public string? Username { get; }

        /// <summary>
        ///     Gets or sets the previous username of the member, if changed.
        /// </summary>
        public string? PreviousUsername { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the member was removed.
        /// </summary>
        public bool Removed { get; }

        /// <summary>
        ///     Gets a value indicating whether any indexable field changed as part of the originating save.
        /// </summary>
        /// <remarks>
        ///     Defaults to <c>true</c> for backward compatibility. When explicitly set to <c>false</c>
        ///     (e.g. by a login-only update when <c>SecuritySettings.TreatLoginAsMemberUpdate</c> is
        ///     <c>false</c>), Examine indexing handlers will skip re-indexing this payload.
        /// </remarks>
        public bool IndexableFieldsChanged { get; } = true;
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Member Cache Refresher";

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        ClearCache(payloads);
        base.RefreshInternal(payloads);
    }

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        ClearCache(new JsonPayload(id, null, false));
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        ClearCache(new JsonPayload(id, null, false));
        base.Remove(id);
    }

    private void ClearCache(params JsonPayload[] payloads)
    {
        _memberPartialViewCacheInvalidator.ClearPartialViewCacheItems(payloads.Select(p => p.Id));

        Attempt<IAppPolicyCache?> memberCache = AppCaches.IsolatedCaches.Get<IMember>();

        foreach (JsonPayload p in payloads)
        {
            _idKeyMap.ClearCache(p.Id);
            if (memberCache.Success is false)
            {
                continue;
            }

            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, int>(p.Id));
            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(p.Username));

            // This specific cache key was introduced to fix an issue where the member username could not be the same as the member id, because the cache keys collided.
            // This is done in a bit of a hacky way, because the cache key is created internally in the repository, but we need to clear it here.
            // Ideally, we want to use a shared way of generating the key between this and the repository.
            // Additionally, the RepositoryCacheKeys actually caches the string to avoid re-allocating memory; we would like to also use this in the repository
            // See:
            // https://github.com/umbraco/Umbraco-CMS/pull/17350
            // https://github.com/umbraco/Umbraco-CMS/pull/17815
            memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(CacheKeys.MemberUserNameCachePrefix + p.Username));

            // If provided, clear the cache by the previous user name too.
            if (string.IsNullOrEmpty(p.PreviousUsername) is false)
            {
                memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(p.PreviousUsername));
                memberCache.Result?.Clear(RepositoryCacheKeys.GetKey<IMember, string>(CacheKeys.MemberUserNameCachePrefix + p.PreviousUsername));
            }
        }
    }
}
