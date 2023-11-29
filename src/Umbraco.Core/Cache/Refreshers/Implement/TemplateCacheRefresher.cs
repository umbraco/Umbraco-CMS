using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Cache;

public sealed class TemplateCacheRefresher : CacheRefresherBase<TemplateCacheRefresherNotification>
{
    public static readonly Guid UniqueId = Guid.Parse("DD12B6A0-14B9-46e8-8800-C154F74047C8");

    private readonly IContentTypeCommonRepository _contentTypeCommonRepository;
    private readonly IIdKeyMap _idKeyMap;

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

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Template Cache Refresher";

    public override void Refresh(int id)
    {
        RemoveFromCache(id);
        base.Refresh(id);
    }

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
