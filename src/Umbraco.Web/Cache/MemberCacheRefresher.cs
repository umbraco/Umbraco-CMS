using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.cms.businesslogic.member;
using umbraco.interfaces;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// A cache refresher to ensure member cache is updated when members change
    /// </summary>
    /// <remarks>
    /// This is not intended to be used directly in your code and it should be sealed but due to legacy code we cannot seal it.
    /// </remarks>
    public class MemberCacheRefresher : CacheRefresherBase<MemberCacheRefresher>
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

        private void ClearCache(int id)
        {
            ApplicationContext.Current.ApplicationCache.
                ClearCacheByKeySearch(string.Format("{0}_{1}", CacheKeys.MemberLibraryCacheKey, id));
            ApplicationContext.Current.ApplicationCache.
                ClearCacheByKeySearch(string.Format("{0}{1}", CacheKeys.MemberBusinessLogicCacheKey, id));
        }
    }
}