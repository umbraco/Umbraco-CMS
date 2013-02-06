using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using Umbraco.Web.Cache;


namespace Umbraco.Web.Strategies.Publishing
{
    /// <summary>
    /// Represents the UpdateCacheAfterUnPublish class, which subscribes to the UnPublished event
    /// of the <see cref="PublishingStrategy"/> class and is responsible for doing the actual
    /// cache refresh after a content item has been unpublished.
    /// </summary>
    /// <remarks>
    /// This implementation is meant as a seperation of the cache refresh from the ContentService
    /// and PublishingStrategy.
    /// This event subscriber will only be relevant as long as there is an xml cache.
    /// </remarks>
    public class UpdateCacheAfterUnPublish : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PublishingStrategy.UnPublished += PublishingStrategy_UnPublished;
        }
        
        void PublishingStrategy_UnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any())
            {
                if (e.PublishedEntities.Count() > 1)
                {
                    foreach (var c in e.PublishedEntities)
                    {
                        UnPublishSingle(c);
                    }
                }
                else
                {
                    var content = e.PublishedEntities.FirstOrDefault();
                    UnPublishSingle(content);
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for a single node by removing it
        /// </summary>
        private void UnPublishSingle(IContent content)
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                DistributedCache.Instance.RemovePageCache(content.Id);
            }
            else
            {
                global::umbraco.content.Instance.ClearDocumentCache(content.Id);
            }
        }
    }
}