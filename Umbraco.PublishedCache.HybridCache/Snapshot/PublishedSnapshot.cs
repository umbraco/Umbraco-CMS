using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Infrastructure.HybridCache.Snapshot;

// implements published snapshot
public class PublishedSnapshot : IPublishedSnapshot
{
    private bool _defaultPreview;
    private PublishedSnapshotElements? _elements;
    private readonly IPublishedSnapshotElementsFactory _publishedSnapshotElementsFactory;

    public PublishedSnapshot(IPublishedSnapshotElementsFactory publishedSnapshotPublishedSnapshotElementsFactory, bool defaultPreview)
    {
        _defaultPreview = defaultPreview;
        _publishedSnapshotElementsFactory = publishedSnapshotPublishedSnapshotElementsFactory;
        // _elementsCache = new FastDictionaryAppCache();
        // var domainStore = new SnapDictionary<int, Domain>();
        // _domainCache = new DomainCache(domainStore.CreateSnapshot(), defaultCultureAccessor.DefaultCulture);
    }

    private PublishedSnapshotElements Elements
    {
        get
        {
            if (_publishedSnapshotElementsFactory == null)
            {
                throw new InvalidOperationException(
                    $"The {typeof(PublishedSnapshot)} cannot be used when the {typeof(IPublishedSnapshotElementsFactory)} is not the default type {typeof(PublishedSnapshotElementsFactory)}");
            }

            return _elements ??= _publishedSnapshotElementsFactory.CreateElements(_defaultPreview);
        }
    }

    public IAppCache? SnapshotCache => throw new NotImplementedException();

    public IAppCache? ElementsCache => Elements.ElementsCache;

    public IPublishedContentCache? Content => Elements.ContentCache;

    public IPublishedMediaCache? Media => Elements.MediaCache;

    public IPublishedMemberCache? Members => Elements.MemberCache;

    public IDomainCache? Domains => Elements.DomainCache;

    public IDisposable ForcedPreview(bool preview, Action<bool>? callback = null) => new ForcedPreviewObject(this, preview, callback);

    private class ForcedPreviewObject : DisposableObjectSlim
    {
        private readonly Action<bool>? _callback;
        private readonly bool _origPreview;
        private readonly PublishedSnapshot _publishedSnapshot;

        public ForcedPreviewObject(PublishedSnapshot publishedShapshot, bool preview, Action<bool>? callback)
        {
            _publishedSnapshot = publishedShapshot;
            _callback = callback;

            // save and force
            _origPreview = publishedShapshot._defaultPreview;
            publishedShapshot._defaultPreview = preview;
        }

        protected override void DisposeResources()
        {
            // restore
            _publishedSnapshot._defaultPreview = _origPreview;
            _callback?.Invoke(_origPreview);
        }
    }

    public void Dispose()
    {
    }
}
