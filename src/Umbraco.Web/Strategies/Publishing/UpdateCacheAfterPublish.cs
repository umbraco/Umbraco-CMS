using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Web.Cache;

namespace Umbraco.Web.Strategies.Publishing
{
    /// <summary>
    /// Represents the UpdateCacheAfterPublish class, which subscribes to the Published event
    /// of the <see cref="PublishingStrategy"/> class and is responsible for doing the actual
    /// cache refresh after a content item has been published.
    /// </summary>
    /// <remarks>
    /// This implementation is meant as a seperation of the cache refresh from the ContentService
    /// and PublishingStrategy.
    /// This event subscriber will only be relevant as long as there is an xml cache.
    /// </remarks>
    public class UpdateCacheAfterPublish : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }
        
        void PublishingStrategy_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any())
            {
                if (e.IsAllRepublished)
                {
                    UpdateEntireCache();
                    return;
                }

                if (e.PublishedEntities.Count() > 1)
                {
                    UpdateMultipleContentCache(e.PublishedEntities);
                }
                else
                {
                    var content = e.PublishedEntities.FirstOrDefault();
                    UpdateSingleContentCache(content);
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for all nodes
        /// </summary>
        private void UpdateEntireCache()
        {
            DistributedCache.Instance.RefreshAllPageCache();
        }

        /// <summary>
        /// Refreshes the xml cache for nodes in list
        /// </summary>
        private void UpdateMultipleContentCache(IEnumerable<IContent> content)
        {
            DistributedCache.Instance.RefreshPageCache(content.ToArray());          
        }

        /// <summary>
        /// Refreshes the xml cache for a single node
        /// </summary>
        private void UpdateSingleContentCache(IContent content)
        {
            DistributedCache.Instance.RefreshPageCache(content);
        }
    }
}