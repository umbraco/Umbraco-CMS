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
            base.Remove(id);
        }

        private void RemoveFromCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(
                string.Format("{0}{1}", CacheKeys.TemplateFrontEndCacheKey, id));

            //need to clear the runtime cache for template instances
            //NOTE: This is temp until we implement the correct ApplicationCache and then we can remove the RuntimeCache, etc...
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<ITemplate>();
        }

    }
}