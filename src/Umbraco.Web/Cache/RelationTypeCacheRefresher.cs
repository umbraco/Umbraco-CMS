using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class RelationTypeCacheRefresher : CacheRefresherBase<RelationTypeCacheRefresher>
    {
        public RelationTypeCacheRefresher(CacheHelper cacheHelper) 
            : base(cacheHelper)
        { }

        #region Define

        protected override RelationTypeCacheRefresher Instance => this;

        public static readonly Guid UniqueId = Guid.Parse("D8375ABA-4FB3-4F86-B505-92FBA1B6F7C9");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Relation Type Cache Refresher";

        #endregion

        #region Refresher

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IRelationType>();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            var cache = CacheHelper.IsolatedRuntimeCache.GetCache<IRelationType>();
            if (cache) cache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IRelationType>(id));
            base.Refresh(id);
        }

        public override void Refresh(Guid id)
        {
            throw new NotSupportedException();
            //base.Refresh(id);
        }

        public override void Remove(int id)
        {
            var cache = CacheHelper.IsolatedRuntimeCache.GetCache<IRelationType>();
            if (cache) cache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IRelationType>(id));
            base.Remove(id);
        }

        #endregion
    }
}
