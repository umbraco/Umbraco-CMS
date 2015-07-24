using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

using umbraco.cms.businesslogic.member;
using Umbraco.Core.Persistence.Repositories;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure member cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MemberCacheRefresher : TypedCacheRefresherBase<MemberCacheRefresher, IMember>
    {

        protected override MemberCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.MemberCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Clears Member Cache"; }
        }
        
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
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.IdToKeyCacheKey);
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheByKeySearch(CacheKeys.KeyToIdCacheKey);
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();

            ApplicationContext.Current.ApplicationCache.RuntimeCache.
                ClearCacheByKeySearch(string.Format("{0}_{1}", CacheKeys.MemberLibraryCacheKey, id));
            ApplicationContext.Current.ApplicationCache.RuntimeCache.
                ClearCacheByKeySearch(string.Format("{0}{1}", CacheKeys.MemberBusinessLogicCacheKey, id));

            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheItem(RepositoryBase.GetCacheIdKey<IMember>(id));
        }
    }
}