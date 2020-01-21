using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Services;

namespace Umbraco.Web.Cache
{
    public sealed class MemberCacheRefresher : TypedCacheRefresherBase<MemberCacheRefresher, IMember>
    {
        private readonly AppCaches _appCaches;
        private readonly IIdkMap _idkMap;

        public MemberCacheRefresher(AppCaches appCaches, IIdkMap idkMap)
            : base(appCaches)
        {
            _appCaches = appCaches;
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
            _appCaches.ClearPartialViewCache();

            var memberCache = AppCaches.IsolatedCaches.Get<IMember>();
            if (memberCache)
                memberCache.Result.Clear(RepositoryCacheKeys.GetKey<IMember>(id));
        }

        #endregion

        #region Indirect

        public static void RefreshMemberTypes(AppCaches appCaches)
        {
            appCaches.IsolatedCaches.ClearCache<IMember>();
        }

        #endregion
    }
}
