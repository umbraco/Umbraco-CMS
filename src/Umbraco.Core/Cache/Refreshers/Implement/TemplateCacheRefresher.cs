using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for template caches.
/// </summary>
public sealed class TemplateCacheRefresher : CacheRefresherBase<TemplateCacheRefresherNotification>
{
    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("DD12B6A0-14B9-46e8-8800-C154F74047C8");

    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
    private readonly IIdKeyMap _idKeyMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="idKeyMap">The ID/key map service.</param>
    /// <param name="contentTypeCommonRepository">The content type common repository.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public TemplateCacheRefresher(
        AppCaches appCaches,
        IIdKeyMap idKeyMap,
        IContentTypeCommonRepository contentTypeCommonRepository,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, eventAggregator, factory)
    {
        _idKeyMap = idKeyMap;
        _contentTypeCommonRepository = contentTypeCommonRepository;
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Template Cache Refresher";

    /// <inheritdoc />
    public override void Refresh(int id)
    {
        RemoveFromCache(id);
        base.Refresh(id);
    }

    /// <inheritdoc />
    public override void Remove(int id)
    {
        RemoveFromCache(id);

        // During removal we need to clear the runtime cache for templates, content and content type instances!!!
        // all three of these types are referenced by templates, and the cache needs to be cleared on every server,
        // otherwise things like looking up content type's after a template is removed is still going to show that
        // it has an associated template.
        ClearAllIsolatedCacheByEntityType<IContent>();
        ClearAllIsolatedCacheByEntityType<IContentType>();
        _contentTypeCommonRepository.ClearCache();

        base.Remove(id);
    }

    private void RemoveFromCache(int id)
    {
        _idKeyMap.ClearCache(id);
        AppCaches.RuntimeCache.Clear($"{CacheKeys.TemplateFrontEndCacheKey}{id}");

        // need to clear the runtime cache for templates
        ClearAllIsolatedCacheByEntityType<ITemplate>();
    }
}
