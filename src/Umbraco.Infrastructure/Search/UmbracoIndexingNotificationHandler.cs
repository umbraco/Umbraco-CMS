using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search
{

    /// <summary>
    /// Event binding in order to keep indexes in sync with Umbraco data.
    /// </summary>
    public sealed class UmbracoIndexingNotificationHandler :
        INotificationHandler<ContentCacheRefresherNotification>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<MediaCacheRefresherNotification>,
        INotificationHandler<MemberCacheRefresherNotification>,
        INotificationHandler<LanguageCacheRefresherNotification>
    {
        private readonly IContentService _contentService;
        private readonly IMemberService _memberService;
        private readonly IMediaService _mediaService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IIndexRebuilder _indexRebuilder;
        private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

        public UmbracoIndexingNotificationHandler(
            IContentService contentService,
            IMemberService memberService,
            IMediaService mediaService,
            IMemberTypeService memberTypeService,
            IIndexRebuilder indexRebuilder,
            IUmbracoIndexingHandler umbracoIndexingHandler)
        {
            _contentService = contentService;
            _memberService = memberService;
            _mediaService = mediaService;
            _memberTypeService = memberTypeService;
            _indexRebuilder = indexRebuilder;
            _umbracoIndexingHandler = umbracoIndexingHandler;
        }

        #region Cache refresher updated event handlers

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

        public void Handle(MemberCacheRefresherNotification args)
        {
            if (!_umbracoIndexingHandler.Enabled)
            {
                return;
            }

            if (Suspendable.ExamineEvents.CanIndex == false)
            {
                return;
            }

            switch (args.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = _memberService.GetById((int)args.MessageObject);
                    if (c1 != null)
                    {
                        _umbracoIndexingHandler.ReIndexForMember(c1);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    _umbracoIndexingHandler.DeleteIndexForEntity((int)args.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    if (args.MessageObject is IMember c3)
                    {
                        _umbracoIndexingHandler.ReIndexForMember(c3);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    if (args.MessageObject is IMember c4)
                    {
                        _umbracoIndexingHandler.DeleteIndexForEntity(c4.Id, false);
                    }
                    break;
                case MessageType.RefreshByPayload:
                    var payload = (MemberCacheRefresher.JsonPayload[])args.MessageObject;
                    foreach (var p in payload)
                    {
                        if (p.Removed)
                        {
                            _umbracoIndexingHandler.DeleteIndexForEntity(p.Id, false);
                        }
                        else
                        {
                            var m = _memberService.GetById(p.Id);
                            if (m != null)
                            {
                                _umbracoIndexingHandler.ReIndexForMember(m);
                            }
                        }
                    }
                    break;
                case MessageType.RefreshAll:
                case MessageType.RefreshByJson:
                default:
                    //We don't support these, these message types will not fire for unpublished content
                    break;
            }
        }

        public void Handle(MediaCacheRefresherNotification args)
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

            foreach (var payload in (MediaCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // remove from *all* indexes
                    _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    var media = _mediaService.GetById(payload.Id);
                    if (media == null)
                    {
                        // gone fishing, remove entirely
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    if (media.Trashed)
                    {
                        _umbracoIndexingHandler.DeleteIndexForEntity(payload.Id, true);
                    }

                    // just that media
                    _umbracoIndexingHandler.ReIndexForMedia(media, !media.Trashed);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = _mediaService.GetPagedDescendants(media.Id, page++, pageSize, out total);
                            foreach (var descendant in descendants)
                            {
                                _umbracoIndexingHandler.ReIndexForMedia(descendant, !descendant.Trashed);
                            }
                        }
                    }
                }
            }
        }

        public void Handle(LanguageCacheRefresherNotification args)
        {
            if (!_umbracoIndexingHandler.Enabled)
            {
                return;
            }

            if (!(args.MessageObject is LanguageCacheRefresher.JsonPayload[] payloads))
            {
                return;
            }

            if (payloads.Length == 0)
            {
                return;
            }

            var removedOrCultureChanged = payloads.Any(x =>
                x.ChangeType == LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture
                    || x.ChangeType == LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove);

            if (removedOrCultureChanged)
            {
                //if a lang is removed or it's culture has changed, we need to rebuild the indexes since
                //field names and values in the index have a string culture value.
                _indexRebuilder.RebuildIndexes(false);
            }
        }

        /// <summary>
        /// Updates indexes based on content type changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Handle(ContentTypeCacheRefresherNotification args)
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

            var changedIds = new Dictionary<string, (List<int> removedIds, List<int> refreshedIds, List<int> otherIds)>();

            foreach (var payload in (ContentTypeCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (!changedIds.TryGetValue(payload.ItemType, out var idLists))
                {
                    idLists = (removedIds: new List<int>(), refreshedIds: new List<int>(), otherIds: new List<int>());
                    changedIds.Add(payload.ItemType, idLists);
                }

                if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Remove))
                {
                    idLists.removedIds.Add(payload.Id);
                }
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain))
                {
                    idLists.refreshedIds.Add(payload.Id);
                }
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther))
                {
                    idLists.otherIds.Add(payload.Id);
                }
            }

            foreach (var ci in changedIds)
            {
                if (ci.Value.refreshedIds.Count > 0 || ci.Value.otherIds.Count > 0)
                {
                    switch (ci.Key)
                    {
                        case var itemType when itemType == typeof(IContentType).Name:
                            RefreshContentOfContentTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct().ToArray());
                            break;
                        case var itemType when itemType == typeof(IMediaType).Name:
                            RefreshMediaOfMediaTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct().ToArray());
                            break;
                        case var itemType when itemType == typeof(IMemberType).Name:
                            RefreshMemberOfMemberTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct().ToArray());
                            break;
                    }
                }

                //Delete all content of this content/media/member type that is in any content indexer by looking up matched examine docs
                _umbracoIndexingHandler.DeleteDocumentsForContentTypes(ci.Value.removedIds);
            }
        }

        private void RefreshMemberOfMemberTypes(int[] memberTypeIds)
        {
            const int pageSize = 500;

            IEnumerable<IMemberType> memberTypes = _memberTypeService.GetAll(memberTypeIds);
            foreach (IMemberType memberType in memberTypes)
            {
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    IEnumerable<IMember> memberToRefresh = _memberService.GetAll(
                        page++, pageSize, out total, "LoginName", Direction.Ascending,
                        memberType.Alias);

                    foreach (IMember c in memberToRefresh)
                    {
                        _umbracoIndexingHandler.ReIndexForMember(c);
                    }
                }
            }
        }

        private void RefreshMediaOfMediaTypes(int[] mediaTypeIds)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IMedia> mediaToRefresh = _mediaService.GetPagedOfTypes(
                    //Re-index all content of these types
                    mediaTypeIds,
                    page++, pageSize, out total, null,
                    Ordering.By("Path", Direction.Ascending));

                foreach (IMedia c in mediaToRefresh)
                {
                    _umbracoIndexingHandler.ReIndexForMedia(c, c.Trashed == false);
                }
            }
        }

        private void RefreshContentOfContentTypes(int[] contentTypeIds)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                IEnumerable<IContent> contentToRefresh = _contentService.GetPagedOfTypes(
                    //Re-index all content of these types
                    contentTypeIds,
                    page++, pageSize, out total, null,
                    //order by shallowest to deepest, this allows us to check it's published state without checking every item
                    Ordering.By("Path", Direction.Ascending));

                //track which Ids have their paths are published
                var publishChecked = new Dictionary<int, bool>();

                foreach (IContent c in contentToRefresh)
                {
                    var isPublished = false;
                    if (c.Published)
                    {
                        if (!publishChecked.TryGetValue(c.ParentId, out isPublished))
                        {
                            //nothing by parent id, so query the service and cache the result for the next child to check against
                            isPublished = _contentService.IsPathPublished(c);
                            publishChecked[c.Id] = isPublished;
                        }
                    }

                    _umbracoIndexingHandler.ReIndexForContent(c, isPublished);
                }
            }
        }

        #endregion

    }
}
