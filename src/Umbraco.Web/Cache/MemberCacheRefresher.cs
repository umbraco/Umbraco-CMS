using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Cache
{
    public sealed class MemberCacheRefresher : TypedCacheRefresherBase<MemberCacheRefresher, IMember>
    {
        public MemberCacheRefresher(CacheHelper cacheHelper)
            : base(cacheHelper)
        { }

        #region Define

        protected override MemberCacheRefresher This => this;

        public static readonly Guid UniqueId = Guid.Parse("E285DF34-ACDC-4226-AE32-C0CB5CF388DA");

        public override Guid RefresherUniqueId => UniqueId;

        public override string Name => "Member Cache Refresher";

        #endregion

        #region Refresher

        public override void Refresh(int id)
        {
            ClearCache(id);
            base.Refresh(id);
        }

        public override void Remove(int id)
        {
            ClearCache(id);
            base.Remove(id);
        }

        public override void Refresh(IMember instance)
        {
            ClearCache(instance.Id);
            base.Refresh(instance);
        }

        public override void Remove(IMember instance)
        {
            ClearCache(instance.Id);
            base.Remove(instance);
        }

        private void ClearCache(int id)
        {
            CacheHelper.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            CacheHelper.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            CacheHelper.ClearPartialViewCache();

            CacheHelper.RuntimeCache.ClearCacheByKeySearch($"{CacheKeys.MemberLibraryCacheKey}_{id}");
            CacheHelper.RuntimeCache.ClearCacheByKeySearch($"{CacheKeys.MemberBusinessLogicCacheKey}{id}");

            var memberCache = CacheHelper.IsolatedRuntimeCache.GetCache<IMember>();
            if (memberCache)
                memberCache.Result.ClearCacheItem(RepositoryBase.GetCacheIdKey<IMember>(id));
        }

        #endregion

        #region Indirect

        public static void RefreshMemberTypes(CacheHelper cacheHelper)
        {
            cacheHelper.IsolatedRuntimeCache.ClearCache<IMember>();
        }

        #endregion
    }
}