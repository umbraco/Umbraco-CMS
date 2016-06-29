using System;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Cache;

namespace Umbraco.Web.PublishedCache
{
    abstract class FacadeServiceBase : IFacadeService
    {
        protected FacadeServiceBase(IFacadeAccessor facadeAccessor)
        {
            FacadeAccessor = facadeAccessor;
        }

        public IFacadeAccessor FacadeAccessor { get; }

        // note: NOT setting _facadeAccessor.Facade here because it is the
        // responsibility of the caller to manage what the 'current' facade is
        public abstract IFacade CreateFacade(string previewToken);

        protected IFacade CurrentFacade => FacadeAccessor.Facade;

        public abstract IPublishedProperty CreateFragmentProperty(PublishedPropertyType propertyType, Guid itemKey, bool previewing, PropertyCacheLevel referenceCacheLevel, object sourceValue = null);

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
