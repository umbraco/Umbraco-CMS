using System;
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
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Services.Implement;
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
        //fixme - we are injecting this which is nice, but we still use ExamineManager everywhere, we could instead interface IExamineManager?
        private IExamineIndexCollectionAccessor _indexCollection;
        private static bool _disableExamineIndexing = false;
        private static volatile bool __isConfigured = false;
        private static readonly object IsConfiguredLocker = new object();

        public void Initialize(IRuntimeState runtime, PropertyEditorCollection propertyEditors, IExamineIndexCollectionAccessor indexCollection, ProfilingLogger profilingLogger)
        {
            _indexCollection = indexCollection;

            //fixme we cannot inject MainDom since it's internal, so thsi is the only way we can get it, alternatively we can add the container to the container and resolve
            //directly from the container but that's not nice either
            if (!(runtime is RuntimeState coreRuntime))
                throw new NotSupportedException($"Unsupported IRuntimeState implementation {runtime.GetType().FullName}, expecting {typeof(RuntimeState).FullName}.");

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
            var examineShutdownRegistered = coreRuntime.MainDom.Register(() =>
            {
                using (profilingLogger.TraceDuration<ExamineComponent>("Examine shutting down"))
                {
                    //Due to the way Examine's own IRegisteredObject works, we'll first run it with immediate=false and then true so that
                    //it's correct subroutines are executed (otherwise we'd have to run this logic manually ourselves)
                    ExamineManager.Instance.Stop(false);
                    ExamineManager.Instance.Stop(true);
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

            var registeredIndexers = indexCollection.Indexes.Values.OfType<UmbracoExamineIndexer>().Count(x => x.EnableDefaultEventHandler);

            profilingLogger.Logger.Info<ExamineComponent>($"Adding examine event handlers for {registeredIndexers} index providers.");

            // don't bind event handlers if we're not suppose to listen
            if (registeredIndexers == 0)
                return;

            BindGridToExamine(profilingLogger.Logger, indexCollection, propertyEditors);

            // bind to distributed cache events - this ensures that this logic occurs on ALL servers
            // that are taking part in a load balanced environment.
            ContentCacheRefresher.CacheUpdated += ContentCacheRefresherUpdated;
            MediaCacheRefresher.CacheUpdated += MediaCacheRefresherUpdated;
            MemberCacheRefresher.CacheUpdated += MemberCacheRefresherUpdated;

            // fixme - content type?
            // events handling removed in ef013f9d3b945d0a48a306ff1afbd49c10c3fff8
            // because, could not make sense of it?

            EnsureUnlocked(profilingLogger.Logger, indexCollection);

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
                    RebuildIndexes(true, _indexCollection, logger);
                }
                catch (Exception e)
                {
                    logger.Error<ExamineComponent>("Failed to rebuild empty indexes.", e);
                }
            });
            bg.Start();
        }

        /// <summary>
        /// Used to rebuild indexes on startup or cold boot
        /// </summary>
        /// <param name="onlyEmptyIndexes"></param>
        /// <param name="indexCollection"></param>
        /// <param name="logger"></param>
        internal static void RebuildIndexes(bool onlyEmptyIndexes, IExamineIndexCollectionAccessor indexCollection, ILogger logger)
        {
            //do not attempt to do this if this has been disabled since we are not the main dom.
            //this can be called during a cold boot
            if (_disableExamineIndexing) return;

            EnsureUnlocked(logger, indexCollection);

            if (onlyEmptyIndexes)
            {
                foreach (var indexer in indexCollection.Indexes.Values.Where(x => x.IsIndexNew()))
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
        private static void EnsureUnlocked(ILogger logger, IExamineIndexCollectionAccessor indexCollection)
        {
            if (_disableExamineIndexing) return;
            if (__isConfigured) return;

            lock (IsConfiguredLocker)
            {
                //double chekc
                if (__isConfigured) return;

                __isConfigured = true;

                foreach (var luceneIndexer in indexCollection.Indexes.Values.OfType<LuceneIndexer>())
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
                        logger.Info<ExamineComponent>("Forcing index " + luceneIndexer.Name + " to be unlocked since it was left in a locked state");
                        IndexWriter.Unlock(dir);
                    }
                }
            }
        }

        private static void BindGridToExamine(ILogger logger, IExamineIndexCollectionAccessor indexCollection, IEnumerable propertyEditors)
        {
            //bind the grid property editors - this is a hack until http://issues.umbraco.org/issue/U4-8437
            try
            {
                var grid = propertyEditors.OfType<GridPropertyEditor>().FirstOrDefault();
                if (grid != null)
                {
                    foreach (var i in indexCollection.Indexes.Values.OfType<UmbracoExamineIndexer>())
                        i.DocumentWriting += grid.DocumentWriting;
                }
            }
            catch (Exception e)
            {
                logger.Error<ExamineComponent>("Failed to bind grid property editor.", e);
            }
        }

        private void MemberCacheRefresherUpdated(MemberCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (Suspendable.ExamineEvents.CanIndex == false)
                return;

            switch (args.MessageType)
            {
                case MessageType.RefreshById:
                    var c1 = Current.Services.MemberService.GetById((int)args.MessageObject);
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

            var mediaService = Current.Services.MediaService;

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

            var contentService = Current.Services.ContentService;

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
            var valueSet = UmbracoContentIndexer.GetValueSets(Current.UrlSegmentProviders, Current.Services.UserService, sender);

            ExamineManager.Instance.IndexItems(
                valueSet.ToArray(),
                _indexCollection.Indexes.Values.OfType<UmbracoContentIndexer>()
                    // only for the specified indexers
                    .Where(x => supportUnpublished.HasValue == false || supportUnpublished.Value == x.SupportUnpublishedContent)
                    .Where(x => x.EnableDefaultEventHandler));
        }

        private void ReIndexForMember(IMember member)
        {
            var valueSet = UmbracoMemberIndexer.GetValueSets(member);

            ExamineManager.Instance.IndexItems(
                valueSet.ToArray(),
                _indexCollection.Indexes.Values.OfType<UmbracoExamineIndexer>()
                    //ensure that only the providers are flagged to listen execute
                    .Where(x => x.EnableDefaultEventHandler));
        }

        private void ReIndexForMedia(IMedia sender, bool isMediaPublished)
        {
            var valueSet = UmbracoContentIndexer.GetValueSets(Current.UrlSegmentProviders, Current.Services.UserService, sender);

            ExamineManager.Instance.IndexItems(
                valueSet.ToArray(),
                _indexCollection.Indexes.Values.OfType<UmbracoContentIndexer>()
                    // index this item for all indexers if the media is not trashed, otherwise if the item is trashed
                    // then only index this for indexers supporting unpublished media
                    .Where(x => isMediaPublished || (x.SupportUnpublishedContent))
                    .Where(x => x.EnableDefaultEventHandler));
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
            ExamineManager.Instance.DeleteFromIndexes(
                entityId.ToString(CultureInfo.InvariantCulture),
                _indexCollection.Indexes.Values.OfType<UmbracoExamineIndexer>()
                    // if keepIfUnpublished == true then only delete this item from indexes not supporting unpublished content,
                    // otherwise if keepIfUnpublished == false then remove from all indexes
                    .Where(x => keepIfUnpublished == false || (x is UmbracoContentIndexer && ((UmbracoContentIndexer)x).SupportUnpublishedContent == false))
                    .Where(x => x.EnableDefaultEventHandler));
        }

    }
}
