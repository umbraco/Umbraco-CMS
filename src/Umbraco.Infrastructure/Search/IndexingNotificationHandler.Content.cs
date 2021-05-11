using System;
using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search
{
    public sealed class ContentIndexingNotificationHandler : INotificationHandler<ContentCacheRefresherNotification>
    {
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;
        private readonly IContentService _contentService;

        public ContentIndexingNotificationHandler(IUmbracoIndexingHandler umbracoIndexingHandler, IContentService contentService)
        {
            _umbracoIndexingHandler = umbracoIndexingHandler ?? throw new ArgumentNullException(nameof(umbracoIndexingHandler));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        }

        /// <summary>
        /// Updates indexes based on content changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Handle(ContentCacheRefresherNotification args)
        {
            if (!_umbracoIndexingHandler.Enabled)
            {
                return;
            }
            if (Suspendable.ExamineEvents.CanIndex == false)
            {
                return;
            }

            if (args.MessageType != MessageType.RefreshByPayload)
            {
                throw new NotSupportedException();
            }

            foreach (var payload in (ContentCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // delete content entirely (with descendants)
                    //  false: remove entirely from all indexes
                    _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!

                    // TODO: Rebuild the index at this point?
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    // don't try to be too clever - refresh entirely
                    // there has to be race conditions in there ;-(

                    var content = _contentService.GetById(payload.Id);
                    if (content == null)
                    {
                        // gone fishing, remove entirely from all indexes (with descendants)
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    IContent published = null;
                    if (content.Published && _contentService.IsPathPublished(content))
                    {
                        published = content;
                    }

                    if (published == null)
                    {
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, true);
                    }

                    // just that content
                    _umbracoIndexingHandler.ReIndexForContent(content, published != null);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var masked = published == null ? null : new List<int>();
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = _contentService.GetPagedDescendants(content.Id, page++, pageSize, out total,
                                //order by shallowest to deepest, this allows us to check it's published state without checking every item
                                ordering: Ordering.By("Path", Direction.Ascending));

                            foreach (var descendant in descendants)
                            {
                                published = null;
                                if (masked != null) // else everything is masked
                                {
                                    if (masked.Contains(descendant.ParentId) || !descendant.Published)
                                    {
                                        masked.Add(descendant.Id);
                                    }
                                    else
                                    {
                                        published = descendant;
                                    }
                                }

                                _umbracoIndexingHandler.ReIndexForContent(descendant, published != null);
                            }
                        }
                    }
                }

                // NOTE
                //
                // DeleteIndexForEntity is handled by UmbracoContentIndexer.DeleteFromIndex() which takes
                //  care of also deleting the descendants
                //
                // ReIndexForContent is NOT taking care of descendants so we have to reload everything
                //  again in order to process the branch - we COULD improve that by just reloading the
                //  XML from database instead of reloading content & re-serializing!
                //
                // BUT ... pretty sure it is! see test "Index_Delete_Index_Item_Ensure_Heirarchy_Removed"
            }
        }
    }
}
