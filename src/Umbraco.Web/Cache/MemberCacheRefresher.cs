using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;

namespace Umbraco.Web.Cache
{
    public sealed class MemberCacheRefresher : TypedCacheRefresherBase<MemberCacheRefresher, IMember>
    {
        private readonly IdkMap _idkMap;

        public MemberCacheRefresher(CacheHelper cacheHelper, IdkMap idkMap)
            : base(cacheHelper)
        {
            _idkMap = idkMap;
        }

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
            _idkMap.ClearCache(id);
            CacheHelper.ClearPartialViewCache();

            var memberCache = CacheHelper.IsolatedRuntimeCache.GetCache<IMember>();
            if (memberCache)
                memberCache.Result.ClearCacheItem(RepositoryCacheKeys.GetKey<IMember>(id));
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
