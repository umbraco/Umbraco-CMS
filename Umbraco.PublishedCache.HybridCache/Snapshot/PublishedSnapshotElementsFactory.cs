using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

internal class PublishedSnapshotElementsFactory : IPublishedSnapshotElementsFactory
{
    private readonly IPublishedContentHybridCache _contentHybridCache;
    private readonly IPublishedMediaHybridCache _mediaHybridCache;
    private readonly IPublishedMemberHybridCache _memberHybridCache;
    private readonly IDefaultCultureAccessor _defaultCultureAccessor;
    private IAppCache? _elementsCache;
    private SnapDictionary<int, Domain> _domainStore;

    public PublishedSnapshotElementsFactory(
        IPublishedContentHybridCache contentHybridCache,
        IPublishedMediaHybridCache mediaHybridCache,
        IPublishedMemberHybridCache memberHybridCache,
        IDefaultCultureAccessor defaultCultureAccessor)
    {
        _contentHybridCache = contentHybridCache;
        _mediaHybridCache = mediaHybridCache;
        _memberHybridCache = memberHybridCache;
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
            ContentCache = _contentHybridCache,
            MediaCache = _mediaHybridCache,
            MemberCache = _memberHybridCache,
            DomainCache = new DomainCache(domainSnap, defaultCulture),
        };
    }
}
