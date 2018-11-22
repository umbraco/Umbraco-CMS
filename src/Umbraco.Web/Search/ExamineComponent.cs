using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.PropertyEditors;
using Umbraco.Examine;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Web.Scheduling;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Search
{

    /// <summary>
    /// Configures and installs Examine.
    /// </summary>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class ExamineComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IExamineManager _examineManager;
        private static bool _disableExamineIndexing = false;
        private static volatile bool _isConfigured = false;
        private static readonly object IsConfiguredLocker = new object();
        private IScopeProvider _scopeProvider;
        private ServiceContext _services;
        private static BackgroundTaskRunner<IBackgroundTask> _rebuildOnStartupRunner;
        private static readonly object _rebuildLocker = new object();
        private IEnumerable<IUrlSegmentProvider> _urlSegmentProviders;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" runs after us
        // but greater that SafeXmlReaderWriter priority which is 60
        private const int EnlistPriority = 80;

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Container.RegisterSingleton<IUmbracoIndexesBuilder, UmbracoIndexesBuilder>();
        }

        internal void Initialize(IRuntimeState runtime, MainDom mainDom, PropertyEditorCollection propertyEditors, IExamineManager examineManager, ProfilingLogger profilingLogger, IScopeProvider scopeProvider, IUmbracoIndexesBuilder indexBuilder, ServiceContext services, IEnumerable<IUrlSegmentProvider> urlSegmentProviders)
        {
            _services = services;
            _scopeProvider = scopeProvider;
            _examineManager = examineManager;
            _urlSegmentProviders = urlSegmentProviders;

            //We want to manage Examine's appdomain shutdown sequence ourselves so first we'll disable Examine's default behavior
            //and then we'll use MainDom to control Examine's shutdown
            ExamineManager.DisableDefaultHostingEnvironmentRegistration();

            //we want to tell examine to use a different fs lock instead of the default NativeFSFileLock which could cause problems if the appdomain
            //terminates and in some rare cases would only allow unlocking of the file if IIS is forcefully terminated. Instead we'll rely on the simplefslock
            //which simply checks the existence of the lock file
            DirectoryTracker.DefaultLockFactory = d =>
            {
                var simpleFsLockFactory = new NoPrefixSimpleFsLockFactory(d);
                return simpleFsLockFactory;
            };

            //let's deal with shutting down Examine with MainDom
            var examineShutdownRegistered = mainDom.Register(() =>
            {
                using (profilingLogger.TraceDuration<ExamineComponent>("Examine shutting down"))
                {
                    _examineManager.Dispose();
                }
            });

            if (!examineShutdownRegistered)
            {
                profilingLogger.Logger.Debug<ExamineComponent>("Examine shutdown not registered, this appdomain is not the MainDom, Examine will be disabled");

                //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled!
                Suspendable.ExamineEvents.SuspendIndexers();
                _disableExamineIndexing = true;
                return; //exit, do not continue
            }

            //create the indexes and register them with the manager
            foreach(var index in indexBuilder.Create())
            {
                _examineManager.AddIndexer(index.Key, index.Value);
            }

            profilingLogger.Logger.Debug<ExamineComponent>("Examine shutdown registered with MainDom");

            var registeredIndexers = examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>().Count(x => x.EnableDefaultEventHandler);

            profilingLogger.Logger.Info<ExamineComponent>("Adding examine event handlers for {RegisteredIndexers} index providers.", registeredIndexers);

            // don't bind event handlers if we're not suppose to listen
            if (registeredIndexers == 0)
                return;

            BindGridToExamine(profilingLogger.Logger, examineManager, propertyEditors);

            // bind to distributed cache events - this ensures that this logic occurs on ALL servers
            // that are taking part in a load balanced environment.
            ContentCacheRefresher.CacheUpdated += ContentCacheRefresherUpdated;
            ContentTypeCacheRefresher.CacheUpdated += ContentTypeCacheRefresherUpdated; ;
            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
            MemberCacheRefresher.CacheUpdated += MemberCacheRefresherUpdated;

            EnsureUnlocked(profilingLogger.Logger, examineManager);

            RebuildIndexes(examineManager, profilingLogger.Logger, true, 5000);
        }
        

        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        /// <param name="logger"></param>
        public static void RebuildIndexes(IExamineManager examineManager, ILogger logger, bool onlyEmptyIndexes, int waitMilliseconds = 0)
        {
            //TODO: need a way to disable rebuilding on startup

            lock(_rebuildLocker)
            {
                if (_rebuildOnStartupRunner != null && _rebuildOnStartupRunner.IsRunning)
                {
                    logger.Warn<ExamineComponent>("Call was made to RebuildIndexes but the task runner for rebuilding is already running");
                    return;
                }

                logger.Info<ExamineComponent>("Starting initialize async background thread.");
                //do the rebuild on a managed background thread
                var task = new RebuildOnStartupTask(examineManager, logger, onlyEmptyIndexes, waitMilliseconds);

                _rebuildOnStartupRunner = new BackgroundTaskRunner<IBackgroundTask>(
                    "RebuildIndexesOnStartup",
                    //new BackgroundTaskRunnerOptions{ LongRunning= true }, //fixme, this flag doesn't have any affect anymore
                    logger);

                _rebuildOnStartupRunner.TryAdd(task);
            }
        }

        

        /// <summary>
        /// Must be called to each index is unlocked before any indexing occurs
        /// </summary>
        /// <remarks>
        /// Indexing rebuilding can occur on a normal boot if the indexes are empty or on a cold boot by the database server messenger. Before
        /// either of these happens, we need to configure the indexes.
        /// </remarks>
        private static void EnsureUnlocked(ILogger logger, IExamineManager examineManager)
        {
            if (_disableExamineIndexing) return;
            if (_isConfigured) return;

            lock (IsConfiguredLocker)
            {
                //double chekc
                if (_isConfigured) return;

                _isConfigured = true;

                foreach (var luceneIndexer in examineManager.IndexProviders.Values.OfType<LuceneIndexer>())
                {
                    //We now need to disable waiting for indexing for Examine so that the appdomain is shutdown immediately and doesn't wait for pending
                    //indexing operations. We used to wait for indexing operations to complete but this can cause more problems than that is worth because
                    //that could end up halting shutdown for a very long time causing overlapping appdomains and many other problems.
                    luceneIndexer.WaitForIndexQueueOnShutdown = false;

                    //we should check if the index is locked ... it shouldn't be! We are using simple fs lock now and we are also ensuring that
                    //the indexes are not operational unless MainDom is true
                    var dir = luceneIndexer.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        logger.Info<ExamineComponent>("Forcing index {IndexerName} to be unlocked since it was left in a locked state", luceneIndexer.Name);
                        IndexWriter.Unlock(dir);
                    }
                }
            }
        }

        private static void BindGridToExamine(ILogger logger, IExamineManager examineManager, IEnumerable propertyEditors)
        {
            //bind the grid property editors - this is a hack until http://issues.umbraco.org/issue/U4-8437
            try
            {
                var grid = propertyEditors.OfType<GridPropertyEditor>().FirstOrDefault();
                if (grid != null)
                {
                    foreach (var i in examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>())
                        i.DocumentWriting += grid.DocumentWriting;
                }
            }
            catch (Exception ex)
            {
                logger.Error<ExamineComponent>(ex, "Failed to bind grid property editor.");
            }
        }

        #region Cache refresher updated event handlers
        private void MemberCacheRefresherUpdated(MemberCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

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
                case MessageType.RefreshAll:
                case MessageType.RefreshByJson:
                default:
                    //We don't support these, these message types will not fire for unpublished content
                    break;
            }
        }

        private void MediaCacheRefresherUpdated(MediaCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

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
                    if (media == null || media.Trashed)
                    {
                        // gone fishing, remove entirely
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    // just that media
                    ReIndexForMedia(media, media.Trashed == false);

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
                                ReIndexForMedia(descendant, descendant.Trashed == false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates indexes based on content type changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentTypeCacheRefresherUpdated(ContentTypeCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            var changedIds = new Dictionary<string, (List<int> removedIds, List<int> refreshedIds, List<int> otherIds)>();

            foreach (var payload in (ContentTypeCacheRefresher.JsonPayload[])args.MessageObject)
            {
                if (!changedIds.TryGetValue(payload.ItemType, out var idLists))
                {
                    idLists = (removedIds: new List<int>(), refreshedIds: new List<int>(), otherIds: new List<int>());
                    changedIds.Add(payload.ItemType, idLists);
                }

                if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.Remove))
                    idLists.removedIds.Add(payload.Id);
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshMain))
                    idLists.refreshedIds.Add(payload.Id);
                else if (payload.ChangeTypes.HasType(ContentTypeChangeTypes.RefreshOther))
                    idLists.otherIds.Add(payload.Id);
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
                    foreach (var index in _examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>())
                    {
                        var searcher = index.GetSearcher();

                        var page = 0;
                        var total = long.MaxValue;
                        while (page * pageSize < total)
                        {
                            //paging with examine, see https://shazwazza.com/post/paging-with-examine/
                            var results = searcher.Search(
                                    searcher.CreateCriteria().Field("nodeType", id).Compile(),
                                    maxResults: pageSize * (page + 1));
                            total = results.TotalItemCount;
                            var paged = results.Skip(page * pageSize);

                            foreach (var item in paged)
                                if (int.TryParse(item.Id, out var contentId))
                                    DeleteIndexForEntity(contentId, false);
                        }
                    }
                }
            }
        }

        private void RefreshMemberOfMemberTypes(int[] memberTypeIds)
        {
            const int pageSize = 500;

            var memberTypes = _services.MemberTypeService.GetAll(memberTypeIds);
            foreach (var memberType in memberTypes)
            {
                var page = 0;
                var total = long.MaxValue;
                while (page * pageSize < total)
                {
                    var memberToRefresh = _services.MemberService.GetAll(
                        page++, pageSize, out total, "LoginName", Direction.Ascending,
                        memberType.Alias);

                    foreach (var c in memberToRefresh)
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
                var mediaToRefresh = _services.MediaService.GetPagedOfTypes(
                    //Re-index all content of these types
                    mediaTypeIds,
                    page++, pageSize, out total, null,
                    Ordering.By("Path", Direction.Ascending));

                foreach (var c in mediaToRefresh)
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
                var contentToRefresh = _services.ContentService.GetPagedOfTypes(
                    //Re-index all content of these types
                    contentTypeIds,
                    page++, pageSize, out total, null,
                    //order by shallowest to deepest, this allows us to check it's published state without checking every item
                    Ordering.By("Path", Direction.Ascending));

                //track which Ids have their paths are published
                var publishChecked = new Dictionary<int, bool>();

                foreach (var c in contentToRefresh)
                {
                    IContent published = null;
                    if (c.Published)
                    {
                        if (publishChecked.TryGetValue(c.ParentId, out var isPublished))
                        {
                            //if the parent's published path has already been verified then this is published
                            if (isPublished)
                                published = c;
                        }
                        else
                        {
                            //nothing by parent id, so query the service and cache the result for the next child to check against
                            isPublished = _services.ContentService.IsPathPublished(c);
                            publishChecked[c.Id] = isPublished;
                            if (isPublished)
                                published = c;
                        }
                    }

                    ReIndexForContent(c, published);
                }
            }
        }

        /// <summary>
        /// Updates indexes based on content changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ContentCacheRefresherUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

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

                    //fixme: Rebuild the index at this point?
                }
                else // RefreshNode or RefreshBranch (maybe trashed)
                {
                    // don't try to be too clever - refresh entirely
                    // there has to be race conds in there ;-(

                    var content = contentService.GetById(payload.Id);
                    if (content == null || content.Trashed)
                    {
                        // gone fishing, remove entirely from all indexes (with descendants)
                        DeleteIndexForEntity(payload.Id, false);
                        continue;
                    }

                    IContent published = null;
                    if (content.Published && contentService.IsPathPublished(content))
                        published = content;

                    // just that content
                    ReIndexForContent(content, published);

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
                                        masked.Add(descendant.Id);
                                    else
                                        published = descendant;
                                }

                                ReIndexForContent(descendant, published);
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
        #endregion

        #region ReIndex/Delete for entity
        private void ReIndexForContent(IContent content, IContent published)
        {
            if (published != null && content.VersionId == published.VersionId)
            {
                ReIndexForContent(content); // same = both
            }
            else
            {
                if (published == null)
                {
                    // remove 'published' - keep 'draft'
                    DeleteIndexForEntity(content.Id, true);
                }
                else
                {
                    // index 'published' - don't overwrite 'draft'
                    ReIndexForContent(published, false);
                }
                ReIndexForContent(content, true); // index 'draft'
            }
        }

        private void ReIndexForContent(IContent sender, bool? supportUnpublished = null)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForContent(this, sender, supportUnpublished));
            else
                DeferedReIndexForContent.Execute(this, sender, supportUnpublished);
        }

        private void ReIndexForMember(IMember member)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForMember(this, member));
            else
                DeferedReIndexForMember.Execute(this, member);
        }

        private void ReIndexForMedia(IMedia sender, bool isMediaPublished)
        {
            var actions = DeferedActions.Get(_scopeProvider);
            if (actions != null)
                actions.Add(new DeferedReIndexForMedia(this, sender, isMediaPublished));
            else
                DeferedReIndexForMedia.Execute(this, sender, isMediaPublished);
        }

        /// <summary>
        /// Remove items from any index that doesn't support unpublished content
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
                actions.Add(new DeferedDeleteIndex(this, entityId, keepIfUnpublished));
            else
                DeferedDeleteIndex.Execute(this, entityId, keepIfUnpublished);
        }
        #endregion

        #region Defered Actions
        private class DeferedActions
        {
            private readonly List<DeferedAction> _actions = new List<DeferedAction>();

            public static DeferedActions Get(IScopeProvider scopeProvider)
            {
                var scopeContext = scopeProvider.Context;

                return scopeContext?.Enlist("examineEvents",
                    () => new DeferedActions(), // creator
                    (completed, actions) => // action
                    {
                        if (completed) actions.Execute();
                    }, EnlistPriority);
            }

            public void Add(DeferedAction action)
            {
                _actions.Add(action);
            }

            private void Execute()
            {
                foreach (var action in _actions)
                    action.Execute();
            }
        }

        private abstract class DeferedAction
        {
            public virtual void Execute()
            { }
        }

        private class DeferedReIndexForContent : DeferedAction
        {
            private readonly ExamineComponent _examineComponent;
            private readonly IContent _content;
            private readonly bool? _supportUnpublished;

            public DeferedReIndexForContent(ExamineComponent examineComponent, IContent content, bool? supportUnpublished)
            {
                _examineComponent = examineComponent;
                _content = content;
                _supportUnpublished = supportUnpublished;
            }

            public override void Execute()
            {
                Execute(_examineComponent, _content, _supportUnpublished);
            }

            public static void Execute(ExamineComponent examineComponent, IContent content, bool? supportUnpublished)
            {
                var valueSet = UmbracoContentIndexer.GetValueSets(examineComponent._urlSegmentProviders, examineComponent._services.UserService, content);

                examineComponent._examineManager.IndexItems(
                    valueSet.ToArray(),
                    examineComponent._examineManager.IndexProviders.Values.OfType<UmbracoContentIndexer>()
                        // only for the specified indexers
                        .Where(x => supportUnpublished.HasValue == false || supportUnpublished.Value == x.SupportUnpublishedContent)
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }

        private class DeferedReIndexForMedia : DeferedAction
        {
            private readonly ExamineComponent _examineComponent;
            private readonly IMedia _media;
            private readonly bool _isPublished;

            public DeferedReIndexForMedia(ExamineComponent examineComponent, IMedia media, bool isPublished)
            {
                _examineComponent = examineComponent;
                _media = media;
                _isPublished = isPublished;
            }

            public override void Execute()
            {
                Execute(_examineComponent, _media, _isPublished);
            }

            public static void Execute(ExamineComponent examineComponent, IMedia media, bool isPublished)
            {
                var valueSet = UmbracoContentIndexer.GetValueSets(examineComponent._urlSegmentProviders, examineComponent._services.UserService, media);

                examineComponent._examineManager.IndexItems(
                    valueSet.ToArray(),
                    examineComponent._examineManager.IndexProviders.Values.OfType<UmbracoContentIndexer>()
                        // index this item for all indexers if the media is not trashed, otherwise if the item is trashed
                        // then only index this for indexers supporting unpublished media
                        .Where(x => isPublished || (x.SupportUnpublishedContent))
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }

        private class DeferedReIndexForMember : DeferedAction
        {
            private readonly ExamineComponent _examineComponent;
            private readonly IMember _member;

            public DeferedReIndexForMember(ExamineComponent examineComponent, IMember member)
            {
                _examineComponent = examineComponent;
                _member = member;
            }

            public override void Execute()
            {
                Execute(_examineComponent, _member);
            }

            public static void Execute(ExamineComponent examineComponent, IMember member)
            {
                var valueSet = UmbracoMemberIndexer.GetValueSets(member);

                examineComponent._examineManager.IndexItems(
                    valueSet.ToArray(),
                    examineComponent._examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>()
                        //ensure that only the providers are flagged to listen execute
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }

        private class DeferedDeleteIndex : DeferedAction
        {
            private readonly ExamineComponent _examineComponent;
            private readonly int _id;
            private readonly bool _keepIfUnpublished;

            public DeferedDeleteIndex(ExamineComponent examineComponent, int id, bool keepIfUnpublished)
            {
                _examineComponent = examineComponent;
                _id = id;
                _keepIfUnpublished = keepIfUnpublished;
            }

            public override void Execute()
            {
                Execute(_examineComponent, _id, _keepIfUnpublished);
            }

            public static void Execute(ExamineComponent examineComponent, int id, bool keepIfUnpublished)
            {
                examineComponent._examineManager.DeleteFromIndexes(
                    id.ToString(CultureInfo.InvariantCulture),
                    examineComponent._examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>()
                        // if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
                        // otherwise if keepIfUnpublished == false then remove from all indexes
                        .Where(x => keepIfUnpublished == false || (x is UmbracoContentIndexer && ((UmbracoContentIndexer)x).SupportUnpublishedContent == false))
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }
        #endregion

        /// <summary>
        /// Background task used to rebuild empty indexes on startup
        /// </summary>
        private class RebuildOnStartupTask : IBackgroundTask
        {
            private readonly IExamineManager _examineManager;
            private readonly ILogger _logger;
            private readonly bool _onlyEmptyIndexes;
            private readonly int _waitMilliseconds;

            public RebuildOnStartupTask(IExamineManager examineManager, ILogger logger, bool onlyEmptyIndexes, int waitMilliseconds = 0)
            {
                _examineManager = examineManager;
                _logger = logger;
                _onlyEmptyIndexes = onlyEmptyIndexes;
                _waitMilliseconds = waitMilliseconds;
            }

            public bool IsAsync => false;

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Run()
            {
                try
                {
                    // rebuilds indexes
                    RebuildIndexes();
                }
                catch (Exception ex)
                {
                    _logger.Error<ExamineComponent>(ex, "Failed to rebuild empty indexes.");
                }
            }

            public Task RunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Used to rebuild indexes on startup or cold boot
            /// </summary>
            /// <param name="onlyEmptyIndexes"></param>
            private void RebuildIndexes()
            {
                //do not attempt to do this if this has been disabled since we are not the main dom.
                //this can be called during a cold boot
                if (_disableExamineIndexing) return;

                if (_waitMilliseconds > 0)
                    Thread.Sleep(_waitMilliseconds);

                EnsureUnlocked(_logger, _examineManager);

                if (_onlyEmptyIndexes)
                {
                    foreach (var indexer in _examineManager.IndexProviders.Values.Where(x => x.IsIndexNew()))
                    {
                        indexer.RebuildIndex();
                    }
                }
                else
                {
                    //do all of them
                    _examineManager.RebuildIndexes();
                }
            }
        }
    }
}
