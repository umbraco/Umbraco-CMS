using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{

    /// <summary>
    /// A cache refresher to ensure template cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class TemplateCacheRefresher : CacheRefresherBase<TemplateCacheRefresher>
    {
        protected override TemplateCacheRefresher Instance
        {
            get { return this; }
        }

        public override string Name
        {
            get
            {
                return "Template cache refresher";
            }
        }

        public override Guid UniqueIdentifier
        {
            get
            {
                return new Guid(DistributedCache.TemplateRefresherId);
            }
        }

        public override void Refresh(int id)
        {
            RemoveFromCache(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            RemoveFromCache(id);

            //During removal we need to clear the runtime cache for templates, content and content type instances!!!
            // all three of these types are referenced by templates, and the cache needs to be cleared on every server,
            // otherwise things like looking up content type's after a template is removed is still going to show that
            // it has an associated template.
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContent>();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<IContentType>();

            base.Remove(id);
        }

        private void RemoveFromCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                string.Format("{0}{1}", CacheKeys.TemplateFrontEndCacheKey, id));

            //need to clear the runtime cache for templates
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<ITemplate>();
        }

    }
}