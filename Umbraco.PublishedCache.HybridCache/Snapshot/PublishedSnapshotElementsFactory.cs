using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

internal class PublishedSnapshotElementsFactory : IPublishedSnapshotElementsFactory
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IMemberCacheService _memberCacheService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly IPublishedContentCacheAccessor _publishedContentCacheAccessor;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;
    private IAppCache? _elementsCache;
    private SnapDictionary<int, Domain> _domainStore;

    public PublishedSnapshotElementsFactory(
        IContentCacheService contentCacheService,
        IMediaCacheService mediaCacheService,
        IMemberCacheService memberCacheService,
        IIdKeyMap idKeyMap,
        IPublishedContentCacheAccessor publishedContentCacheAccessor,
        IDefaultCultureAccessor defaultCultureAccessor)
    {
        _contentCacheService = contentCacheService;
        _mediaCacheService = mediaCacheService;
        _memberCacheService = memberCacheService;
        _idKeyMap = idKeyMap;
        _publishedContentCacheAccessor = publishedContentCacheAccessor;
        _defaultCultureAccessor = defaultCultureAccessor;
        _domainStore = new SnapDictionary<int, Domain>();
    }

    public PublishedSnapshotElements CreateElements(bool preview)
    {
        SnapDictionary<int, Domain>.Snapshot domainSnap = _domainStore.CreateSnapshot();
        var defaultCulture = _defaultCultureAccessor.DefaultCulture;
        return new PublishedSnapshotElements
        {
            ElementsCache = _elementsCache ??= new FastDictionaryAppCache(),
            ContentCache = new ContentCache(_contentCacheService, _idKeyMap, _publishedContentCacheAccessor),
            MediaCache = new MediaCache(_mediaCacheService, _publishedContentCacheAccessor),
            MemberCache = new MemberCache(_memberCacheService, _publishedContentCacheAccessor),
            DomainCache = new DomainCache(domainSnap, defaultCulture),
        };
    }
}
