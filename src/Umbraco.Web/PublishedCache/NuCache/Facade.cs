using System;
using Umbraco.Core.Cache;

namespace Umbraco.Web.PublishedCache.NuCache
{
    // implements the facade
    class Facade : IFacade, IDisposable
    {
        private readonly FacadeService _service;
        private readonly bool _defaultPreview;
        private FacadeElements _elements;

        #region Constructors

        public Facade(FacadeService service, bool defaultPreview)
        {
            _service = service;
            _defaultPreview = defaultPreview;
            FacadeCache = new ObjectCacheRuntimeCacheProvider();
        }

        public class FacadeElements : IDisposable
        {
            public ContentCache ContentCache;
            public MediaCache MediaCache;
            public MemberCache MemberCache;
            public DomainCache DomainCache;
            public ICacheProvider FacadeCache;
            public ICacheProvider SnapshotCache;

            public void Dispose()
            {
                ContentCache.Dispose();
                MediaCache.Dispose();
            }
        }

        private FacadeElements Elements => _elements ?? (_elements = _service.GetElements(_defaultPreview));

        public void Resync()
        {
            // no lock - facades are single-thread
            _elements?.Dispose();
            _elements = null;
        }

        #endregion

        #region Caches

        public ICacheProvider FacadeCache { get; private set; }

        public ICacheProvider SnapshotCache => Elements.SnapshotCache;

        #endregion

        #region IFacade

        public IPublishedContentCache ContentCache => Elements.ContentCache;

        public IPublishedMediaCache MediaCache => Elements.MediaCache;

        public IPublishedMemberCache MemberCache => Elements.MemberCache;

        public IDomainCache DomainCache => Elements.DomainCache;

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _elements?.Dispose();
        }

        #endregion
    }
}
