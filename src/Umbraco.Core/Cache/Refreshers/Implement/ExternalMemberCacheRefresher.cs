// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for external member caches.
/// </summary>
public sealed class ExternalMemberCacheRefresher : PayloadCacheRefresherBase<ExternalMemberCacheRefresherNotification, ExternalMemberCacheRefresher.JsonPayload>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("A1B2C3D4-5E6F-4A8B-9C0D-E1F2A3B4C5D6");

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    public ExternalMemberCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <summary>
    ///     Represents the JSON payload for external member cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The integer identifier of the external member.</param>
        /// <param name="key">The unique key of the external member.</param>
        /// <param name="removed">Whether the external member was removed.</param>
        public JsonPayload(int id, Guid key, bool removed)
        {
            Id = id;
            Key = key;
            Removed = removed;
        }

        /// <summary>
        ///     Gets the integer identifier of the external member (used as the Examine document ID).
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the unique key of the external member.
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        ///     Gets a value indicating whether the external member was removed.
        /// </summary>
        public bool Removed { get; }
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "External Member Cache Refresher";

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        // External members have no content cache to clear.
        base.RefreshInternal(payloads);
    }
}
