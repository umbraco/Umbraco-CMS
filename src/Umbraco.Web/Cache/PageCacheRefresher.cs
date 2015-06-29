using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Sync;
using umbraco;
using umbraco.cms.businesslogic.web;

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
            content.Instance.RefreshContentFromDatabaseAsync();
            base.RefreshAll();
        }

        /// <summary>
        /// Refreshes the cache for the node with specified id
        /// </summary>
        /// <param name="id">The id.</param>
        public override void Refresh(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
            content.Instance.UpdateDocumentCache(id);
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
            base.Refresh(id);
        }

        /// <summary>
        /// Removes the node with the specified id from the cache
        /// </summary>
        /// <param name="id">The id.</param>
        public override void Remove(int id)
        {
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
            content.Instance.ClearDocumentCache(id);
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<PublicAccessEntry>();
            base.Remove(id);
        }

        public override void Refresh(IContent instance)
        {
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
            content.Instance.UpdateDocumentCache(new Document(instance));
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<PublicAccessEntry>();
            base.Refresh(instance);
        }

        public override void Remove(IContent instance)
        {
            ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
            content.Instance.ClearDocumentCache(new Document(instance));
            DistributedCache.Instance.ClearAllMacroCacheOnCurrentServer();
            DistributedCache.Instance.ClearXsltCacheOnCurrentServer();
            ApplicationContext.Current.ApplicationCache.RuntimeCache.ClearCacheObjectTypes<PublicAccessEntry>();
            base.Remove(instance);
        }
    }
}
