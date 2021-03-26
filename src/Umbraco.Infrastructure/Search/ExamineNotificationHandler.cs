using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Search
{
    public sealed class ExamineNotificationHandler :
        INotificationHandler<UmbracoApplicationStarting>,
        INotificationHandler<ContentCacheRefresherNotification>,
        INotificationHandler<ContentTypeCacheRefresherNotification>,
        INotificationHandler<MediaCacheRefresherNotification>,
        INotificationHandler<MemberCacheRefresherNotification>,
        INotificationHandler<LanguageCacheRefresherNotification>
    {
        private readonly IExamineManager _examineManager;
        private readonly IContentValueSetBuilder _contentValueSetBuilder;
        private readonly IPublishedContentValueSetBuilder _publishedContentValueSetBuilder;
        private readonly IValueSetBuilder<IMedia> _mediaValueSetBuilder;
        private readonly IValueSetBuilder<IMember> _memberValueSetBuilder;
        private readonly BackgroundIndexRebuilder _backgroundIndexRebuilder;
        private readonly TaskHelper _taskHelper;
        private readonly IRuntimeState _runtimeState;
        private readonly IScopeProvider _scopeProvider;
        private readonly ServiceContext _services;
        private readonly IMainDom _mainDom;
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<ExamineNotificationHandler> _logger;
        private readonly IUmbracoIndexesCreator _indexCreator;
        private static bool s_deactivate_handlers;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" runs after us
        // but greater that SafeXmlReaderWriter priority which is 60
        private const int EnlistPriority = 80;

        public ExamineNotificationHandler(IMainDom mainDom,
            IExamineManager examineManager,
            IProfilingLogger profilingLogger,
            ILogger<ExamineNotificationHandler> logger,
            IScopeProvider scopeProvider,
            IUmbracoIndexesCreator indexCreator,
            ServiceContext services,
            IContentValueSetBuilder contentValueSetBuilder,
            IPublishedContentValueSetBuilder publishedContentValueSetBuilder,
            IValueSetBuilder<IMedia> mediaValueSetBuilder,
            IValueSetBuilder<IMember> memberValueSetBuilder,
            BackgroundIndexRebuilder backgroundIndexRebuilder,
            TaskHelper taskHelper,
            IRuntimeState runtimeState)
        {
            _services = services;
            _scopeProvider = scopeProvider;
            _examineManager = examineManager;
            _contentValueSetBuilder = contentValueSetBuilder;
            _publishedContentValueSetBuilder = publishedContentValueSetBuilder;
            _mediaValueSetBuilder = mediaValueSetBuilder;
            _memberValueSetBuilder = memberValueSetBuilder;
            _backgroundIndexRebuilder = backgroundIndexRebuilder;
            _taskHelper = taskHelper;
            _runtimeState = runtimeState;
            _mainDom = mainDom;
            _profilingLogger = profilingLogger;
            _logger = logger;
            _indexCreator = indexCreator;
        }
        public void Handle(UmbracoApplicationStarting notification)
        {
            //let's deal with shutting down Examine with MainDom
            var examineShutdownRegistered = _mainDom.Register(release: () =>
            {
                using (_profilingLogger.TraceDuration<ExamineNotificationHandler>("Examine shutting down"))
                {
                    _examineManager.Dispose();
                }
            });

            if (!examineShutdownRegistered)
            {
                _logger.LogInformation("Examine shutdown not registered, this AppDomain is not the MainDom, Examine will be disabled");

                //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
                Suspendable.ExamineEvents.SuspendIndexers(_logger);
                return; //exit, do not continue
            }

            //create the indexes and register them with the manager
            foreach (IIndex index in _indexCreator.Create())
            {
                _examineManager.AddIndex(index);
            }

            _logger.LogDebug("Examine shutdown registered with MainDom");

            var registeredIndexers = _examineManager.Indexes.OfType<IUmbracoIndex>().Count(x => x.EnableDefaultEventHandler);

            _logger.LogInformation("Adding examine event handlers for {RegisteredIndexers} index providers.", registeredIndexers);

            // don't bind event handlers if we're not suppose to listen
            if (registeredIndexers == 0)
            {
                s_deactivate_handlers = true;
            }

            if (_mainDom.IsMainDom && _runtimeState.Level >= RuntimeLevel.Run)
            {
                _backgroundIndexRebuilder.RebuildIndexes(true);
            }
        }


        #region Cache refresher updated event handlers

        /// <summary>
        /// Updates indexes based on content changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Handle(ContentCacheRefresherNotification args)
        {
            if (s_deactivate_handlers)
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

            var contentService = _services.ContentService;

            foreach (var payload in (ContentCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // delete content entirely (with descendants)
                    //  false: remove entirely from all indexes
                    DeleteIndexForEntity(payload.Id, false);
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

                    var content = contentService.GetById(payload.Id);
                    if (content == null)
                    {
                        // gone fishing, remove entirely from all indexes (with descendants)
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    IContent published = null;
                    if (content.Published && contentService.IsPathPublished(content))
                    {
                        published = content;
                    }

                    if (published == null)
                    {
                        DeleteIndexForEntity(payload.Id, true);
                    }

                    // just that content
                    ReIndexForContent(content, published != null);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var masked = published == null ? null : new List<int>();
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = contentService.GetPagedDescendants(content.Id, page++, pageSize, out total,
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

                                ReIndexForContent(descendant, published != null);
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
            if (s_deactivate_handlers)
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
                    var c1 = _services.MemberService.GetById((int)args.MessageObject);
                    if (c1 != null)
                    {
                        ReIndexForMember(c1);
                    }
                    break;
                case MessageType.RemoveById:

                    // This is triggered when the item is permanently deleted

                    DeleteIndexForEntity((int)args.MessageObject, false);
                    break;
                case MessageType.RefreshByInstance:
                    if (args.MessageObject is IMember c3)
                    {
                        ReIndexForMember(c3);
                    }
                    break;
                case MessageType.RemoveByInstance:

                    // This is triggered when the item is permanently deleted

                    if (args.MessageObject is IMember c4)
                    {
                        DeleteIndexForEntity(c4.Id, false);
                    }
                    break;
                case MessageType.RefreshByPayload:
                    var payload = (MemberCacheRefresher.JsonPayload[])args.MessageObject;
                    foreach (var p in payload)
                    {
                        if (p.Removed)
                        {
                            DeleteIndexForEntity(p.Id, false);
                        }
                        else
                        {
                            var m = _services.MemberService.GetById(p.Id);
                            if (m != null)
                            {
                                ReIndexForMember(m);
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
            if (s_deactivate_handlers)
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

            var mediaService = _services.MediaService;

            foreach (var payload in (MediaCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (payload.ChangeTypes.HasType(TreeChangeTypes.Remove))
                {
                    // remove from *all* indexes
                    DeleteIndexForEntity(payload.Id, false);
                }
                else if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshAll))
                {
                    // ExamineEvents does not support RefreshAll
                    // just ignore that payload
                    // so what?!
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    var media = mediaService.GetById(payload.Id);
                    if (media == null)
                    {
                        // gone fishing, remove entirely
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    if (media.Trashed)
                    {
                        DeleteIndexForEntity(payload.Id, true);
                    }

                    // just that media
                    ReIndexForMedia(media, !media.Trashed);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        const int pageSize = 500;
                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            var descendants = mediaService.GetPagedDescendants(media.Id, page++, pageSize, out total);
                            foreach (var descendant in descendants)
                            {
                                ReIndexForMedia(descendant, !descendant.Trashed);
                            }
                        }
                    }
                }
            }
        }

        public void Handle(LanguageCacheRefresherNotification args)
        {
            if (s_deactivate_handlers)
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
                _backgroundIndexRebuilder.RebuildIndexes(false);
            }
        }

        /// <summary>
        /// Updates indexes based on content type changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Handle(ContentTypeCacheRefresherNotification args)
        {
            if (s_deactivate_handlers)
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

            const int pageSize = 500;

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
                foreach (var id in ci.Value.removedIds)
                {
                    foreach (var index in _examineManager.Indexes.OfType<IUmbracoIndex>())
                    {
                        var searcher = index.GetSearcher();

                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            //paging with examine, see https://shazwazza.com/post/paging-with-examine/
                            var results = searcher.CreateQuery().Field("nodeType", id.ToInvariantString()).Execute(maxResults: pageSize * (page + 1));
                            total = results.TotalItemCount;
                            var paged = results.Skip(page * pageSize);

                            foreach (ISearchResult item in paged)
                            {
                                if (int.TryParse(item.Id, out int contentId))
                                {
                                    DeleteIndexForEntity(contentId, false);
                                }
                            }

                            page++;
                        }
                    }
                }
            }
        }

        private void RefreshMemberOfMemberTypes(int[] memberTypeIds)
        {
            const int pageSize = 500;

            IEnumerable<IMemberType> memberTypes = _services.MemberTypeService.GetAll(memberTypeIds);
            foreach (IMemberType memberType in memberTypes)
            {
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    IEnumerable<IMember> memberToRefresh = _services.MemberService.GetAll(
                        page++, pageSize, out total, "LoginName", Direction.Ascending,
                        memberType.Alias);

                    foreach (IMember c in memberToRefresh)
                    {
                        ReIndexForMember(c);
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
                IEnumerable<IMedia> mediaToRefresh = _services.MediaService.GetPagedOfTypes(
                    //Re-index all content of these types
                    mediaTypeIds,
                    page++, pageSize, out total, null,
                    Ordering.By("Path", Direction.Ascending));

                foreach (IMedia c in mediaToRefresh)
                {
                    ReIndexForMedia(c, c.Trashed == false);
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
                IEnumerable<IContent> contentToRefresh = _services.ContentService.GetPagedOfTypes(
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
                            isPublished = _services.ContentService.IsPathPublished(c);
                            publishChecked[c.Id] = isPublished;
                        }
                    }

                    ReIndexForContent(c, isPublished);
                }
            }
        }

        #endregion

        #region ReIndex/Delete for entity
        private void ReIndexForContent(IContent sender, bool isPublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
            {
                actions.Add(new DeferedReIndexForContent(_taskHelper, this, sender, isPublished));
            }
            else
            {
                DeferedReIndexForContent.Execute(_taskHelper, this, sender, isPublished);
            }
        }

        private void ReIndexForMember(IMember member)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
            {
                actions.Add(new DeferedReIndexForMember(_taskHelper, this, member));
            }
            else
            {
                DeferedReIndexForMember.Execute(_taskHelper, this, member);
            }
        }

        private void ReIndexForMedia(IMedia sender, bool isPublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
            {
                actions.Add(new DeferedReIndexForMedia(_taskHelper, this, sender, isPublished));
            }
            else
            {
                DeferedReIndexForMedia.Execute(_taskHelper, this, sender, isPublished);
            }
        }

        /// <summary>
        /// Remove items from an index
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="keepIfUnpublished">
        /// If true, indicates that we will only delete this item from indexes that don't support unpublished content.
        /// If false it will delete this from all indexes regardless.
        /// </param>
        private void DeleteIndexForEntity(int entityId, bool keepIfUnpublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
            {
                actions.Add(new DeferedDeleteIndex(this, entityId, keepIfUnpublished));
            }
            else
            {
                DeferedDeleteIndex.Execute(this, entityId, keepIfUnpublished);
            }
        }
        #endregion

        #region Deferred Actions
        private class DeferedActions
        {
            private readonly List<DeferedAction> _actions = new List<DeferedAction>();

            public static DeferedActions Get(IScopeProvider scopeProvider)
            {
                IScopeContext scopeContext = scopeProvider.Context;

                return scopeContext?.Enlist("examineEvents",
                    () => new DeferedActions(), // creator
                    (completed, actions) => // action
                    {
                        if (completed)
                        {
                            actions.Execute();
                        }
                    }, EnlistPriority);
            }

            public void Add(DeferedAction action) => _actions.Add(action);

            private void Execute()
            {
                foreach (DeferedAction action in _actions)
                {
                    action.Execute();
                }
            }
        }

        /// <summary>
        /// An action that will execute at the end of the Scope being completed
        /// </summary>
        private abstract class DeferedAction
        {
            public virtual void Execute()
            { }
        }

        /// <summary>
        /// Re-indexes an <see cref="IContent"/> item on a background thread
        /// </summary>
        private class DeferedReIndexForContent : DeferedAction
        {
            private readonly TaskHelper _taskHelper;
            private readonly ExamineNotificationHandler _ExamineNotificationHandler;
            private readonly IContent _content;
            private readonly bool _isPublished;

            public DeferedReIndexForContent(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IContent content, bool isPublished)
            {
                _taskHelper = taskHelper;
                _ExamineNotificationHandler = ExamineNotificationHandler;
                _content = content;
                _isPublished = isPublished;
            }

            public override void Execute() => Execute(_taskHelper, _ExamineNotificationHandler, _content, _isPublished);

            public static void Execute(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IContent content, bool isPublished)
                => taskHelper.RunBackgroundTask(() =>
                {
                    using IScope scope = ExamineNotificationHandler._scopeProvider.CreateScope(autoComplete: true);

                    // for content we have a different builder for published vs unpublished
                    // we don't want to build more value sets than is needed so we'll lazily build 2 one for published one for non-published
                    var builders = new Dictionary<bool, Lazy<List<ValueSet>>>
                    {
                        [true] = new Lazy<List<ValueSet>>(() => ExamineNotificationHandler._publishedContentValueSetBuilder.GetValueSets(content).ToList()),
                        [false] = new Lazy<List<ValueSet>>(() => ExamineNotificationHandler._contentValueSetBuilder.GetValueSets(content).ToList())
                    };

                    foreach (IUmbracoIndex index in ExamineNotificationHandler._examineManager.Indexes.OfType<IUmbracoIndex>()
                        //filter the indexers
                        .Where(x => isPublished || !x.PublishedValuesOnly)
                        .Where(x => x.EnableDefaultEventHandler))
                    {
                        List<ValueSet> valueSet = builders[index.PublishedValuesOnly].Value;
                        index.IndexItems(valueSet);
                    }

                    return Task.CompletedTask;
                });
        }

        /// <summary>
        /// Re-indexes an <see cref="IMedia"/> item on a background thread
        /// </summary>
        private class DeferedReIndexForMedia : DeferedAction
        {
            private readonly TaskHelper _taskHelper;
            private readonly ExamineNotificationHandler _ExamineNotificationHandler;
            private readonly IMedia _media;
            private readonly bool _isPublished;

            public DeferedReIndexForMedia(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IMedia media, bool isPublished)
            {
                _taskHelper = taskHelper;
                _ExamineNotificationHandler = ExamineNotificationHandler;
                _media = media;
                _isPublished = isPublished;
            }

            public override void Execute() => Execute(_taskHelper, _ExamineNotificationHandler, _media, _isPublished);

            public static void Execute(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IMedia media, bool isPublished) =>
                // perform the ValueSet lookup on a background thread
                taskHelper.RunBackgroundTask(() =>
                {
                    using IScope scope = ExamineNotificationHandler._scopeProvider.CreateScope(autoComplete: true);

                    var valueSet = ExamineNotificationHandler._mediaValueSetBuilder.GetValueSets(media).ToList();

                    foreach (IUmbracoIndex index in ExamineNotificationHandler._examineManager.Indexes.OfType<IUmbracoIndex>()
                        //filter the indexers
                        .Where(x => isPublished || !x.PublishedValuesOnly)
                        .Where(x => x.EnableDefaultEventHandler))
                    {
                        index.IndexItems(valueSet);
                    }

                    return Task.CompletedTask;
                });
        }

        /// <summary>
        /// Re-indexes an <see cref="IMember"/> item on a background thread
        /// </summary>
        private class DeferedReIndexForMember : DeferedAction
        {
            private readonly ExamineNotificationHandler _ExamineNotificationHandler;
            private readonly IMember _member;
            private readonly TaskHelper _taskHelper;

            public DeferedReIndexForMember(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IMember member)
            {
                _ExamineNotificationHandler = ExamineNotificationHandler;
                _member = member;
                _taskHelper = taskHelper;
            }

            public override void Execute() => Execute(_taskHelper, _ExamineNotificationHandler, _member);

            public static void Execute(TaskHelper taskHelper, ExamineNotificationHandler ExamineNotificationHandler, IMember member) =>
                // perform the ValueSet lookup on a background thread
                taskHelper.RunBackgroundTask(() =>
                {
                    using IScope scope = ExamineNotificationHandler._scopeProvider.CreateScope(autoComplete: true);

                    var valueSet = ExamineNotificationHandler._memberValueSetBuilder.GetValueSets(member).ToList();
                    foreach (IUmbracoIndex index in ExamineNotificationHandler._examineManager.Indexes.OfType<IUmbracoIndex>()
                        //filter the indexers
                        .Where(x => x.EnableDefaultEventHandler))
                    {
                        index.IndexItems(valueSet);
                    }

                    return Task.CompletedTask;
                });
        }

        private class DeferedDeleteIndex : DeferedAction
        {
            private readonly ExamineNotificationHandler _ExamineNotificationHandler;
            private readonly int _id;
            private readonly bool _keepIfUnpublished;

            public DeferedDeleteIndex(ExamineNotificationHandler ExamineNotificationHandler, int id, bool keepIfUnpublished)
            {
                _ExamineNotificationHandler = ExamineNotificationHandler;
                _id = id;
                _keepIfUnpublished = keepIfUnpublished;
            }

            public override void Execute() => Execute(_ExamineNotificationHandler, _id, _keepIfUnpublished);

            public static void Execute(ExamineNotificationHandler ExamineNotificationHandler, int id, bool keepIfUnpublished)
            {
                var strId = id.ToString(CultureInfo.InvariantCulture);
                foreach (var index in ExamineNotificationHandler._examineManager.Indexes.OfType<IUmbracoIndex>()
                    .Where(x => x.PublishedValuesOnly || !keepIfUnpublished)
                    .Where(x => x.EnableDefaultEventHandler))
                {
                    index.DeleteFromIndex(strId);
                }
            }
        }
        #endregion
    }
}
