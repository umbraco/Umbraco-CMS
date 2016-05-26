using System;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    abstract class FacadeServiceBase : IFacadeService
    {
        // FIXME need a facade accessor of some sort? to init the facade service! of course!
        private Func<IFacade> _getFacadeFunc = () => UmbracoContext.Current == null ? null : UmbracoContext.Current.Facade;

        public Func<IFacade> GetFacadeFunc
        {
            get { return _getFacadeFunc; }
            set
            {
                using (Resolution.Configuration)
                {
                    _getFacadeFunc = value;
                }
            }
        }

        public abstract IFacade CreateFacade(string previewToken);

        public IFacade GetFacade()
        {
            var caches = _getFacadeFunc();
            if (caches == null)
                throw new Exception("Carrier's caches is null.");
            return caches;
        }

        public abstract string EnterPreview(IUser user, int contentId);
        public abstract void RefreshPreview(string previewToken, int contentId);
        public abstract void ExitPreview(string previewToken);
        public abstract void Notify(ContentCacheRefresher.JsonPayload[] payloads, out bool draftChanged, out bool publishedChanged);
        public abstract void Notify(MediaCacheRefresher.JsonPayload[] payloads, out bool anythingChanged);
        public abstract void Notify(ContentTypeCacheRefresher.JsonPayload[] payloads);
        public abstract void Notify(DataTypeCacheRefresher.JsonPayload[] payloads);
        public abstract void Notify(DomainCacheRefresher.JsonPayload[] payloads);

        public virtual void Dispose()
        { }
    }
}
