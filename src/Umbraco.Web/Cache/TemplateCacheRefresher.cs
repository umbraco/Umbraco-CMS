using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
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
            base.Remove(id);
        }

        private void RemoveFromCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(
                string.Format("{0}{1}", CacheKeys.TemplateFrontEndCacheKey, id));

            ApplicationContext.Current.ApplicationCache.ClearCacheItem(
                string.Format("{0}{1}", CacheKeys.TemplateBusinessLogicCacheKey, id));
        }

    }
}