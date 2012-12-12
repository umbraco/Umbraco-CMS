using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.presentation.cache;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Represents the UpdateCacheAfterPublish class, which subscribes to the Published event
    /// of the <see cref="PublishingStrategy"/> class and is responsible for doing the actual
    /// cache refresh after a content item has been published.
    /// </summary>
    /// <remarks>
    /// This implementation is meant as a seperation of the cache refresh from the ContentService
    /// and PublishingStrategy.
    /// </remarks>
    public class UpdateCacheAfterPublish : IApplicationStartupHandler
    {
        public UpdateCacheAfterPublish()
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        void PublishingStrategy_Published(object sender, Core.PublishingEventArgs e)
        {
            if (sender is IContent)
            {
                var content = sender as IContent;
                UpdateSingleContentCache(content);
            }
            else if (sender is IEnumerable<IContent>)
            {
                if (e.IsAllRepublished)
                {
                    var content = sender as IEnumerable<IContent>;
                    UpdateMultipleContentCache(content);
                }
                else
                {
                    UpdateEntireCache();
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for all nodes
        /// </summary>
        private void UpdateEntireCache()
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                dispatcher.RefreshAll(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"));
            }
            else
            {
                content.Instance.RefreshContentFromDatabaseAsync();
            }
        }

        /// <summary>
        /// Refreshes the xml cache for nodes in list
        /// </summary>
        private void UpdateMultipleContentCache(IEnumerable<IContent> content)
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                foreach (var c in content)
                {
                    dispatcher.Refresh(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"), c.Id);
                }
            }
            else
            {
                var documents = content.Select(x => new Document(x)).ToList();
                global::umbraco.content.Instance.UpdateDocumentCache(documents);
            }
        }

        /// <summary>
        /// Refreshes the xml cache for a single node
        /// </summary>
        private void UpdateSingleContentCache(IContent content)
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                dispatcher.Refresh(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"), content.Id);
            }
            else
            {
                var doc = new Document(content);
                global::umbraco.content.Instance.UpdateDocumentCache(doc);
            }
        }
    }
}