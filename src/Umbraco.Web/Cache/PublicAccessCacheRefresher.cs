using System;
using System.Xml;
using Newtonsoft.Json;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Web.Cache
{
    public sealed class PublicAccessCacheRefresher : CacheRefresherBase<PublicAccessCacheRefresher>
    {
        protected override PublicAccessCacheRefresher Instance
        {
            get { return this; }
        }

        public override Guid UniqueIdentifier
        {
            get { return new Guid(DistributedCache.PublicAccessCacheRefresherId); }
        }

        public override string Name
        {
            get { return "Public access cache refresher"; }
        }

        public override void Refresh(Guid id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Refresh(id);
        }

        public override void Refresh(int id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Refresh(id);
        }

        public override void RefreshAll()
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.RefreshAll();
        }

        public override void Remove(int id)
        {
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
            base.Remove(id);
        }
    }
}