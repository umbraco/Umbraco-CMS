using System;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    abstract class FacadeServiceBase : IFacadeService
    {
        private IFacadeAccessor _facadeAccessor;

        protected FacadeServiceBase(IFacadeAccessor facadeAccessor)
        {
            _facadeAccessor = facadeAccessor;
        }

        // note: NOT setting _facadeAccessor.Facade here because it is the
        // responsibility of the caller to manage what the 'current' facade is
        public abstract IFacade CreateFacade(string previewToken);

        protected IFacade CurrentFacade => _facadeAccessor.Facade;

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
