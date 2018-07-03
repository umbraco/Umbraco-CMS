using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Web.Cache
{
    public sealed class TemplateCacheRefresher : CacheRefresherBase<TemplateCacheRefresher>
    {
        private readonly IdkMap _idkMap;

        public TemplateCacheRefresher(CacheHelper cacheHelper, IdkMap idkMap)
            : base(cacheHelper)
        {
            _idkMap = idkMap;
        }

        #region Define

        protected override TemplateCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("DD12B6A0-14B9-46e8-8800-C154F74047C8");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Template Cache Refresher";

        #endregion

        #region Refresher

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
            ClearAllIsolatedCacheByEntityType<IContent>();
            ClearAllIsolatedCacheByEntityType<IContentType>();

            base.Remove(id);
        }

        private void RemoveFromCache(int id)
        {
            _idkMap.ClearCache(id);
            CacheHelper.RuntimeCache.ClearCacheItem($"{CacheKeys.TemplateFrontEndCacheKey}{id}");

            //need to clear the runtime cache for templates
            ClearAllIsolatedCacheByEntityType<ITemplate>();
        }

        #endregion
    }
}
