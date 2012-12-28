using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using umbraco;
using umbraco.interfaces;
using umbraco.presentation.cache;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Represents the UpdateCacheAfterUnPublish class, which subscribes to the UnPublished event
    /// of the <see cref="PublishingStrategy"/> class and is responsible for doing the actual
    /// cache refresh after a content item has been unpublished.
    /// </summary>
    /// <remarks>
    /// This implementation is meant as a seperation of the cache refresh from the ContentService
    /// and PublishingStrategy.
    /// </remarks>
    public class UpdateCacheAfterUnPublish : IApplicationStartupHandler
    {
        public UpdateCacheAfterUnPublish()
        {
            PublishingStrategy.UnPublished += PublishingStrategy_UnPublished;
        }

        void PublishingStrategy_UnPublished(object sender, PublishingEventArgs e)
        {
            if (sender is IContent)
            {
                var content = sender as IContent;
                UnPublishSingle(content);
            }
            else if (sender is IEnumerable<IContent>)
            {
                var content = sender as IEnumerable<IContent>;
                foreach (var c in content)
                {
                    UnPublishSingle(c);
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
                dispatcher.Remove(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"), content.Id);
            }
            else
            {
                global::umbraco.content.Instance.ClearDocumentCache(content.Id);
            }
        }
    }
}