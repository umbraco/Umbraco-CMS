using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

/// <summary>
///     Discards what the content cache data serializer factory has derived from content type and media type
///     definitions, on every server, when those definitions are refreshed.
/// </summary>
/// <remarks>
///     The notifications handled by <see cref="CacheRefreshingNotificationHandler" /> and
///     <see cref="DeferredCacheRebuildNotificationHandler" /> are published only on the server that saved the type.
///     The cache refresher notification is raised on every server, once the repository caches have been cleared.
/// </remarks>
internal sealed class SerializerTypeCacheRefresherNotificationHandler :
    INotificationHandler<ContentTypeCacheRefresherNotification>
{
    private readonly IContentCacheDataSerializerFactory _serializerFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializerTypeCacheRefresherNotificationHandler" /> class.
    /// </summary>
    /// <param name="serializerFactory">The content cache data serializer factory.</param>
    public SerializerTypeCacheRefresherNotificationHandler(IContentCacheDataSerializerFactory serializerFactory)
        => _serializerFactory = serializerFactory;

    /// <inheritdoc />
    public void Handle(ContentTypeCacheRefresherNotification notification)
        => _serializerFactory.ClearTypeCache();
}
