using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search;

public sealed class ContentTypeIndexingNotificationHandler : INotificationHandler<ContentTypeCacheRefresherNotification>
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IUmbracoIndexingHandler _umbracoIndexingHandler;

    public ContentTypeIndexingNotificationHandler(
        IUmbracoIndexingHandler umbracoIndexingHandler,
        IContentService contentService, IMemberService memberService, IMediaService mediaService,
        IMemberTypeService memberTypeService)
    {
        _umbracoIndexingHandler =
            umbracoIndexingHandler ?? throw new ArgumentNullException(nameof(umbracoIndexingHandler));
        _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        _memberService = memberService ?? throw new ArgumentNullException(nameof(memberService));
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _memberTypeService = memberTypeService ?? throw new ArgumentNullException(nameof(memberTypeService));
    }

    /// <summary>
    ///     Updates indexes based on content type changes
    /// </summary>
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

        foreach (ContentTypeCacheRefresher.JsonPayload payload in (ContentTypeCacheRefresher.JsonPayload[])args
                     .MessageObject)
        {
            if (!changedIds.TryGetValue(
                payload.ItemType,
                out (List<int> removedIds, List<int> refreshedIds, List<int> otherIds) idLists))
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

        foreach (KeyValuePair<string, (List<int> removedIds, List<int> refreshedIds, List<int> otherIds)> ci in
                 changedIds)
        {
            if (ci.Value.refreshedIds.Count > 0 || ci.Value.otherIds.Count > 0)
            {
                switch (ci.Key)
                {
                    case var itemType when itemType == typeof(IContentType).Name:
                        RefreshContentOfContentTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct()
                            .ToArray());
                        break;
                    case var itemType when itemType == typeof(IMediaType).Name:
                        RefreshMediaOfMediaTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct().ToArray());
                        break;
                    case var itemType when itemType == typeof(IMemberType).Name:
                        RefreshMemberOfMemberTypes(ci.Value.refreshedIds.Concat(ci.Value.otherIds).Distinct()
                            .ToArray());
                        break;
                }
            }

            // Delete all content of this content/media/member type that is in any content indexer by looking up matched examine docs
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

                // Re-index all content of these types
                mediaTypeIds,
                page++, pageSize, out total, null,
                Ordering.By("Path"));

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

                // Re-index all content of these types
                contentTypeIds,
                page++, pageSize, out total, null,

                // order by shallowest to deepest, this allows us to check it's published state without checking every item
                Ordering.By("Path"));

            // track which Ids have their paths are published
            var publishChecked = new Dictionary<int, bool>();

            foreach (IContent c in contentToRefresh)
            {
                var isPublished = false;
                if (c.Published)
                {
                    if (!publishChecked.TryGetValue(c.ParentId, out isPublished))
                    {
                        // nothing by parent id, so query the service and cache the result for the next child to check against
                        isPublished = _contentService.IsPathPublished(c);
                        publishChecked[c.Id] = isPublished;
                    }
                }

                _umbracoIndexingHandler.ReIndexForContent(c, isPublished);
            }
        }
    }
}
