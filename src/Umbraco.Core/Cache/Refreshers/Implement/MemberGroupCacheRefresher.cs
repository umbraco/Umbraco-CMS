using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for member group caches.
/// </summary>
public sealed class MemberGroupCacheRefresher : PayloadCacheRefresherBase<MemberGroupCacheRefresherNotification, MemberGroupCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MemberGroupCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public MemberGroupCacheRefresher(AppCaches appCaches, IJsonSerializer jsonSerializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, jsonSerializer, eventAggregator, factory)
    {
    }

    #region Json

    /// <summary>
    ///     Represents the JSON payload for member group cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the member group.</param>
        /// <param name="name">The name of the member group.</param>
        public JsonPayload(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        ///     Gets the name of the member group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the identifier of the member group.
        /// </summary>
        public int Id { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("187F236B-BD21-4C85-8A7C-29FBA3D6C00C");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Member Group Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        ClearCache();
        base.RefreshInternal(payloads);
    }

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        ClearCache();
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        ClearCache();
        base.Remove(id);
    }

    private void ClearCache() =>

        // Since we cache by group name, it could be problematic when renaming to
        // previously existing names - see http://issues.umbraco.org/issue/U4-10846.
        // To work around this, just clear all the cache items
        AppCaches.IsolatedCaches.ClearCache<IMemberGroup>();

    #endregion
}
