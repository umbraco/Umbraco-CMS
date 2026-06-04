using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Provides cache refresh functionality for element containers (folders).
/// </summary>
/// <remarks>
/// A deleted container's node id is never reused, so its key→id mapping in <see cref="IIdKeyMap"/> must be
/// evicted on every server. Otherwise a container recreated under the same key resolves to the stale id and
/// the element tree's children query returns nothing until the next app restart. This refresher only evicts
/// the id/key map - element data is unaffected by container changes, so it deliberately avoids the broader
/// invalidation performed by <see cref="ElementCacheRefresher"/>.
/// </remarks>
public sealed class ElementContainerCacheRefresher : PayloadCacheRefresherBase<ElementContainerCacheRefresherNotification, ElementContainerCacheRefresher.JsonPayload>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerCacheRefresher"/> class.
    /// </summary>
    public ElementContainerCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IIdKeyMap idKeyMap,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
        => _idKeyMap = idKeyMap;

    #region Json

    /// <summary>
    /// Represents a JSON-serializable payload identifying an element container that changed.
    /// </summary>
    public class JsonPayload
    {
        public JsonPayload(int id, Guid key)
        {
            Id = id;
            Key = key;
        }

        /// <summary>
        /// Gets the unique integer identifier for the container.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the unique GUID key associated with the container.
        /// </summary>
        public Guid Key { get; }
    }

    #endregion

    #region Define

    /// <summary>
    /// Represents a unique identifier for the cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("9C9D8B0E-2F1A-4D63-9C2E-7E6B5A4F3C21");

    /// <inheritdoc/>
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc/>
    public override string Name => "Element Container Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc/>
    public override void Refresh(JsonPayload[] payloads)
    {
        foreach (JsonPayload payload in payloads)
        {
            // Clearing by id also evicts the key→id direction, as the id/key map keeps both in sync.
            _idKeyMap.ClearCache(payload.Id);
        }

        base.Refresh(payloads);
    }

    // These events should never trigger. Everything should be PAYLOAD/JSON.

    /// <inheritdoc/>
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
