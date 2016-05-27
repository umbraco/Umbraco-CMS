using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.ObjectResolution;

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

        private FacadeElements Elements
        {
            get
            {
                // no lock - facades are single-thread
                return _elements ?? (_elements = _service.GetElements(_defaultPreview));
            }
        }

        public void Resync()
        {
            // no lock - facades are single-thread
            if (_elements != null)
                _elements.Dispose();
            _elements = null;
        }

        #endregion

        #region Current - for tests

        private static Func<Facade> _getCurrentFacadeFunc = () =>
        {
#if DEBUG
            if (FacadeServiceResolver.HasCurrent == false) return null;
            var service = FacadeServiceResolver.Current.Service as FacadeService;
            if (service == null) return null;
            return (Facade) service.GetFacadeFunc();
#else
            return (Facade) ((FacadeService) FacadeServiceResolver.Current.Service).GetFacadeFunc();
#endif
        };

        public static Func<Facade> GetCurrentFacadeFunc
        {
            get { return _getCurrentFacadeFunc; }
            set
            {
                using (Resolution.Configuration)
                {
                    _getCurrentFacadeFunc = value;
                }
            }
        }

        public static Facade Current
        {
            get { return _getCurrentFacadeFunc(); }
        }

        #endregion

        #region Caches

        public ICacheProvider FacadeCache { get; private set; }

        public ICacheProvider SnapshotCache { get { return Elements.SnapshotCache; } }

        #endregion

        #region IFacade

        public IPublishedContentCache ContentCache { get { return Elements.ContentCache; } }

        public IPublishedMediaCache MediaCache { get { return Elements.MediaCache; } }

        public IPublishedMemberCache MemberCache { get { return Elements.MemberCache; } }

        public IDomainCache DomainCache { get { return Elements.DomainCache; } }

        #endregion

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_elements != null)
                _elements.Dispose();
        }

        #endregion
    }
}
