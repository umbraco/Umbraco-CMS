using System;
using umbraco;
using umbraco.interfaces;
using umbraco.presentation.cache;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// PageCacheRefresher is the standard CacheRefresher used by Load-Balancing in Umbraco.
    /// </summary>
    /// <remarks>
    /// If Load balancing is enabled (by default disabled, is set in umbracoSettings.config) PageCacheRefresher will be called
    /// everytime content is added/updated/removed to ensure that the content cache is identical on all load balanced servers
    /// </remarks>    
    public class PageCacheRefresher : ICacheRefresher
    {       
        /// <summary>
        /// Gets the unique identifier of the CacheRefresher.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid UniqueIdentifier
        {
            get
            {
                return new Guid("27AB3022-3DFA-47b6-9119-5945BC88FD66");
            }
        }

        /// <summary>
        /// Gets the name of the CacheRefresher
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return "Page Refresher"; }
        }

        /// <summary>
        /// Refreshes all nodes in umbraco.
        /// </summary>
        public void RefreshAll()
        {
            content.Instance.RefreshContentFromDatabaseAsync();
        }

        /// <summary>
        /// Not used with content.
        /// </summary>
        /// <param name="id">The id.</param>
        public void Refresh(Guid id)
        {
            // Not used when pages
        }

        /// <summary>
        /// Refreshes the cache for the node with specified id
        /// </summary>
        /// <param name="id">The id.</param>
        public void Refresh(int id)
        {
            content.Instance.UpdateDocumentCache(id);
        }

        /// <summary>
        /// Removes the node with the specified id from the cache
        /// </summary>
        /// <param name="id">The id.</param>
        public void Remove(int id)
        {
            content.Instance.ClearDocumentCache(id);
        }
    }
}
