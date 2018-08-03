using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class RelationTypeCacheRefresher : CacheRefresherBase<RelationTypeCacheRefresher>
    {
        protected override RelationTypeCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return DistributedCache.RelationTypeCacheRefresherGuid; }
        }

        public override string Name
        {
            get { return "Relation Type Cache Refresher"; }
        }

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<IRelationType>();
            base.RefreshAll();
        }

        public override void Refresh(int id)
        {
            var cache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IRelationType>();
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
            var cache = ApplicationContext.Current.ApplicationCache.IsolatedRuntimeCache.GetCache<IRelationType>();
            if (cache) cache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IRelationType>(id));
            base.Remove(id);
        }
    }
}
