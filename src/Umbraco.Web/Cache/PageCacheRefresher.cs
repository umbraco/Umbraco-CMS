using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;
using umbraco;
using umbraco.cms.businesslogic.web;
using Umbraco.Web.PublishedCache.XmlPublishedCache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// PageCacheRefresher is the standard CacheRefresher used by Load-Balancing in Umbraco.
    /// </summary>
    /// <remarks>
    /// If Load balancing is enabled (by default disabled, is set in umbracoSettings.config) PageCacheRefresher will be called
    /// everytime content is added/updated/removed to ensure that the content cache is identical on all load balanced servers
    /// </remarks>
    public class PageCacheRefresher : TypedCacheRefresherBase<PageCacheRefresher, IContent>
    {

        protected override PageCacheRefresher Instance
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the unique identifier of the CacheRefresher.
        /// </summary>
        /// <value>The unique identifier.</value>
        public override Guid UniqueIdentifier
        {
            get
            {
                return new Guid(DistributedCache.PageCacheRefresherId);
            }
        }

        /// <summary>
        /// Gets the name of the CacheRefresher
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "Page Refresher"; }
        }

        /// <summary>
        /// Refreshes all nodes in umbraco.
        /// </summary>
        public override void RefreshAll()
        {
            if (Suspendable.PageCacheRefresher.CanRefreshDocumentCacheFromDatabase)
                content.Instance.RefreshContentFromDatabase();
            ClearCaches();
            base.RefreshAll();
        }

        /// <summary>
        /// Refreshes the cache for the node with specified id
        /// </summary>
        /// <param name="id">The id.</param>
        public override void Refresh(int id)
        {
            if (Suspendable.PageCacheRefresher.CanUpdateDocumentCache)
                content.Instance.UpdateDocumentCache(id);
            ClearCaches();
            base.Refresh(id);
        }

        /// <summary>
        /// Removes the node with the specified id from the cache
        /// </summary>
        /// <param name="id">The id.</param>
        public override void Remove(int id)
        {
            if (Suspendable.PageCacheRefresher.CanUpdateDocumentCache)
                content.Instance.ClearDocumentCache(id, false);
            ClearCaches();
            base.Remove(id);
        }

        public override void Refresh(IContent instance)
        {
            if (Suspendable.PageCacheRefresher.CanUpdateDocumentCache)
                content.Instance.UpdateDocumentCache(new Document(instance));
            ClearCaches();
            base.Refresh(instance);
        }

        public override void Remove(IContent instance)
        {
            if (Suspendable.PageCacheRefresher.CanUpdateDocumentCache)
                content.Instance.ClearDocumentCache(new Document(instance), false);
            ClearCaches();
            base.Remove(instance);
        }

        private void ClearCaches()
        {
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
            XmlPublishedContent.ClearRequest();
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
            ClearAllIsolatedCacheByEntityType<PublicAccessEntry>();
        }
    }
}
