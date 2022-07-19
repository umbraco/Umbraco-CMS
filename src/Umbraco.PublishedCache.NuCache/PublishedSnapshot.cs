using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.PublishedCache;

// implements published snapshot
public class PublishedSnapshot : IPublishedSnapshot
{
    private readonly PublishedSnapshotService? _service;
    private bool _defaultPreview;
    private PublishedSnapshotElements? _elements;

    #region Constructors

    public PublishedSnapshot(IPublishedSnapshotService service, bool defaultPreview)
    {
        _service = service as PublishedSnapshotService;
        _defaultPreview = defaultPreview;
    }

    public class PublishedSnapshotElements : IDisposable
    {
        public void Dispose()
        {
            ContentCache?.Dispose();
            MediaCache?.Dispose();
            MemberCache?.Dispose();
        }
#pragma warning disable IDE1006 // Naming Styles
        public ContentCache? ContentCache;
        public MediaCache? MediaCache;
        public MemberCache? MemberCache;
        public DomainCache? DomainCache;
        public IAppCache? SnapshotCache;
        public IAppCache? ElementsCache;
#pragma warning restore IDE1006 // Naming Styles
    }

    private PublishedSnapshotElements Elements
    {
        get
        {
            if (_service == null)
            {
                throw new InvalidOperationException(
                    $"The {typeof(PublishedSnapshot)} cannot be used when the {typeof(IPublishedSnapshotService)} is not the default type {typeof(PublishedSnapshotService)}");
            }

            return _elements ??= _service.GetElements(_defaultPreview);
        }
    }

    public void Resync()
    {
        // no lock - published snapshots are single-thread
        _elements?.Dispose();
        _elements = null;
    }

    #endregion

    #region Caches

    public IAppCache? SnapshotCache => Elements.SnapshotCache;

    public IAppCache? ElementsCache => Elements.ElementsCache;

    #endregion

    #region IPublishedSnapshot

    public IPublishedContentCache? Content => Elements.ContentCache;

    public IPublishedMediaCache? Media => Elements.MediaCache;

    public IPublishedMemberCache? Members => Elements.MemberCache;

    public IDomainCache? Domains => Elements.DomainCache;

    public IDisposable ForcedPreview(bool preview, Action<bool>? callback = null) =>
        new ForcedPreviewObject(this, preview, callback);

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

    #endregion

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _elements?.Dispose();
    }

    #endregion
}
