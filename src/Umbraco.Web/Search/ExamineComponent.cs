﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Documents;
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
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.PropertyEditors;
using Umbraco.Examine;

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
        private UrlSegmentProviderCollection _urlSegmentProviders;
        private ServiceContext _services;

        // the default enlist priority is 100
        // enlist with a lower priority to ensure that anything "default" runs after us
        // but greater that SafeXmlReaderWriter priority which is 60
        private const int EnlistPriority = 80;

        internal void Initialize(IRuntimeState runtime, MainDom mainDom, PropertyEditorCollection propertyEditors, IExamineManager examineManager, ProfilingLogger profilingLogger, IScopeProvider scopeProvider, UrlSegmentProviderCollection urlSegmentProviderCollection, ServiceContext services)
        {
            _services = services;
            _urlSegmentProviders = urlSegmentProviderCollection;
            _scopeProvider = scopeProvider;
            _examineManager = examineManager;

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
                    ExamineManager.Instance.Dispose();
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
            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
            MemberCacheRefresher.CacheUpdated += MemberCacheRefresherUpdated;

            // fixme - content type?
            // events handling removed in ef013f9d3b945d0a48a306ff1afbd49c10c3fff8
            // because, could not make sense of it?

            EnsureUnlocked(profilingLogger.Logger, examineManager);

            RebuildIndexesOnStartup(profilingLogger.Logger);
        }

        /// <summary>
        /// Called to rebuild empty indexes on startup
        /// </summary>
        /// <param name="logger"></param>
        private void RebuildIndexesOnStartup(ILogger logger)
        {
            //TODO: need a way to disable rebuilding on startup

            logger.Info<ExamineComponent>("Starting initialize async background thread.");

            // make it async in order not to slow down the boot
            // fixme - should be a proper background task else we cannot stop it!
            var bg = new Thread(() =>
            {
                try
                {
                    // rebuilds any empty indexes
                    RebuildIndexes(true, _examineManager, logger);
                }
                catch (Exception ex)
                {
                    logger.Error<ExamineComponent>(ex, "Failed to rebuild empty indexes.");
                }
            });
            bg.Start();
        }

        /// <summary>
        /// Used to rebuild indexes on startup or cold boot
        /// </summary>
        /// <param name="onlyEmptyIndexes"></param>
        /// <param name="examineManager"></param>
        /// <param name="logger"></param>
        internal static void RebuildIndexes(bool onlyEmptyIndexes, IExamineManager examineManager, ILogger logger)
        {
            //do not attempt to do this if this has been disabled since we are not the main dom.
            //this can be called during a cold boot
            if (_disableExamineIndexing) return;

            EnsureUnlocked(logger, examineManager);

            if (onlyEmptyIndexes)
            {
                foreach (var indexer in examineManager.IndexProviders.Values.Where(x => x.IsIndexNew()))
                {
                    indexer.RebuildIndex();
                }
            }
            else
            {
                //do all of them
                ExamineManager.Instance.RebuildIndexes();
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

            foreach (var payload in (MediaCacheRefresher.JsonPayload[]) args.MessageObject)
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
                        var descendants = mediaService.GetDescendants(media);
                        foreach (var descendant in descendants)
                        {
                            ReIndexForMedia(descendant, descendant.Trashed == false);
                        }
                    }
                }
            }
        }

        private void ContentCacheRefresherUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            if (args.MessageType != MessageType.RefreshByPayload)
                throw new NotSupportedException();

            var contentService = _services.ContentService;

            foreach (var payload in (ContentCacheRefresher.JsonPayload[]) args.MessageObject)
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
                    if (content.Published && ((ContentService)contentService).IsPathPublished(content))
                        published = content;

                    // just that content
                    ReIndexForContent(content, published);

                    // branch
                    if (payload.ChangeTypes.HasType(TreeChangeTypes.RefreshBranch))
                    {
                        var masked = published == null ? null : new List<int>();
                        var descendants = contentService.GetDescendants(content);
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

                ExamineManager.Instance.IndexItems(
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

                ExamineManager.Instance.IndexItems(
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

                ExamineManager.Instance.IndexItems(
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
                ExamineManager.Instance.DeleteFromIndexes(
                    id.ToString(CultureInfo.InvariantCulture),
                    examineComponent._examineManager.IndexProviders.Values.OfType<UmbracoExamineIndexer>()
                        // if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
                        // otherwise if keepIfUnpublished == false then remove from all indexes
                        .Where(x => keepIfUnpublished == false || (x is UmbracoContentIndexer && ((UmbracoContentIndexer)x).SupportUnpublishedContent == false))
                        .Where(x => x.EnableDefaultEventHandler));
            }
        }

    }
}
