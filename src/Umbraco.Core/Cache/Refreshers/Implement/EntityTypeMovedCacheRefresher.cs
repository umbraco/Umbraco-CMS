using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Provides cache refresh functionality for content types, media types, member types and data types that have
/// been moved to a different folder.
/// </summary>
/// <remarks>
/// A move only changes the structural node data of an entity - its parent, path and level. The repository cache
/// policies clear the local cache when the entity is saved, but they queue no cache instruction, so without this
/// refresher other servers keep serving entities with a stale path and level for the lifetime of their cache.
/// <para>
/// Only the caches holding whole entities are evicted. Nothing that depends on the shape of a content type or the
/// configuration of a data type is affected by a move, so the far broader invalidation performed by
/// <see cref="ContentTypeCacheRefresher"/> and <see cref="DataTypeCacheRefresher"/> - which clears the content,
/// media and member caches and rebuilds the published content type caches - is deliberately avoided. The id/key
/// map is left alone as well, as a move changes neither the id nor the key.
/// </para>
/// </remarks>
public sealed class EntityTypeMovedCacheRefresher : PayloadCacheRefresherBase<EntityTypeMovedCacheRefresherNotification, EntityTypeMovedCacheRefresher.JsonPayload>
{
    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeMovedCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The serializer used for the refresher payload.</param>
    /// <param name="contentTypeCommonRepository">The repository holding the shared runtime cache of content types.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The factory for creating cache refresher notifications.</param>
    public EntityTypeMovedCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IContentTypeCommonRepository contentTypeCommonRepository,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
        => _contentTypeCommonRepository = contentTypeCommonRepository;

    #region Json

    /// <summary>
    /// Represents a JSON-serializable payload identifying an entity that was moved.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPayload"/> class.
        /// </summary>
        /// <param name="itemType">The name of the moved entity's type, for example <c>IContentType</c>.</param>
        /// <param name="id">The unique integer identifier of the moved entity.</param>
        /// <param name="key">The unique GUID key of the moved entity.</param>
        public JsonPayload(string itemType, int id, Guid key)
        {
            ItemType = itemType;
            Id = id;
            Key = key;
        }

        /// <summary>
        /// Gets the name of the moved entity's type, for example <c>IContentType</c>.
        /// </summary>
        public string ItemType { get; }

        /// <summary>
        /// Gets the unique integer identifier of the moved entity.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the unique GUID key of the moved entity.
        /// </summary>
        public Guid Key { get; }
    }

    #endregion

    #region Define

    /// <summary>
    /// Represents a unique identifier for the cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("5E7C1B94-3A6D-42F0-9E88-1C4B7A2D6F35");

    /// <inheritdoc/>
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc/>
    public override string Name => "Entity Type Moved Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc/>
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        var itemTypes = payloads.Select(payload => payload.ItemType).ToHashSet();

        var hasContentTypes = itemTypes.Contains(nameof(IContentType));
        var hasMediaTypes = itemTypes.Contains(nameof(IMediaType));
        var hasMemberTypes = itemTypes.Contains(nameof(IMemberType));

        if (hasContentTypes || hasMediaTypes || hasMemberTypes)
        {
            // The content type repositories share a runtime cache holding every type, keyed as a whole.
            _contentTypeCommonRepository.ClearCache();
        }

        // These caches hold the full set of types as a single entry, so one clear covers every moved entity.
        if (hasContentTypes)
        {
            ClearAllIsolatedCacheByEntityType<IContentType>();
        }

        if (hasMediaTypes)
        {
            ClearAllIsolatedCacheByEntityType<IMediaType>();
        }

        if (hasMemberTypes)
        {
            ClearAllIsolatedCacheByEntityType<IMemberType>();
        }

        if (itemTypes.Contains(nameof(IDataType)))
        {
            ClearDataTypeCache(payloads);
        }

        base.RefreshInternal(payloads);
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

    private void ClearDataTypeCache(JsonPayload[] payloads)
    {
        Attempt<IAppPolicyCache?> dataTypeCache = AppCaches.IsolatedCaches.Get<IDataType>();
        if (dataTypeCache.Success is false)
        {
            return;
        }

        // Data types are cached per entity, and separately by id and by key, so both must be evicted.
        foreach (JsonPayload payload in payloads.Where(payload => payload.ItemType == nameof(IDataType)))
        {
            dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetKey<IDataType, int>(payload.Id));
            dataTypeCache.Result?.Clear(RepositoryCacheKeys.GetGuidKey<IDataType>(payload.Key));
        }
    }
}
