using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Config;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Umbraco.Core;
using Umbraco.Core.Logging;
using UmbracoExamine;

namespace Umbraco.Web
{
    /// <summary>
    /// Used to configure Examine during startup for the web application
    /// </summary>
    internal class ExamineStartup
    {
        private readonly List<BaseIndexProvider> _indexesToRebuild = new List<BaseIndexProvider>();
        private readonly ApplicationContext _appCtx;
        private readonly ProfilingLogger _profilingLogger;
        private static bool _isConfigured = false;

        //this is used if we are not the MainDom, in which case we need to ensure that if indexes need rebuilding that this
        //doesn't occur since that should only occur when we are MainDom
        private bool _disableExamineIndexing = false;

        public ExamineStartup(ApplicationContext appCtx)
        {
            _appCtx = appCtx;
            _profilingLogger = appCtx.ProfilingLogger;
        }

        /// <summary>
        /// Called during the initialize operation of the boot manager process
        /// </summary>
        public void Initialize()
        {
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

            //This is basically a hack for this item: http://issues.umbraco.org/issue/U4-5976
            // when Examine initializes it will try to rebuild if the indexes are empty, however in many cases not all of Examine's
            // event handlers will be assigned during bootup when the rebuilding starts which is a problem. So with the examine 0.1.58.2941 build
            // it has an event we can subscribe to in order to cancel this rebuilding process, but what we'll do is cancel it and postpone the rebuilding until the
            // boot process has completed. It's a hack but it works.
            ExamineManager.Instance.BuildingEmptyIndexOnStartup += OnInstanceOnBuildingEmptyIndexOnStartup;

            //let's deal with shutting down Examine with MainDom
            var examineShutdownRegistered = _appCtx.MainDom.Register(() =>
            {
                using (_profilingLogger.TraceDuration<ExamineStartup>("Examine shutting down"))
                {
                    //Due to the way Examine's own IRegisteredObject works, we'll first run it with immediate=false and then true so that
                    //it's correct subroutines are executed (otherwise we'd have to run this logic manually ourselves)
                    ExamineManager.Instance.Stop(false);
                    ExamineManager.Instance.Stop(true);
                }
            });
            if (examineShutdownRegistered)
            {
                _profilingLogger.Logger.Debug<ExamineStartup>("Examine shutdown registered with MainDom");
            }
            else
            {
                _profilingLogger.Logger.Debug<ExamineStartup>("Examine shutdown not registered, this appdomain is not the MainDom");

                //if we could not register the shutdown examine ourselves, it means we are not maindom! in this case all of examine should be disabled
                //from indexing anything on startup!!
                _disableExamineIndexing = true;
                Suspendable.ExamineEvents.SuspendIndexers();
            }
        }

        /// <summary>
        /// Called during the Complete operation of the boot manager process
        /// </summary>
        public void Complete()
        {
            EnsureUnlockedAndConfigured();

            //Ok, now that everything is complete we'll check if we've stored any references to index that need rebuilding and run them
            // (see the initialize method for notes) - we'll ensure we remove the event handler too in case examine manager doesn't actually
            // initialize during startup, in which case we want it to rebuild the indexes itself.
            ExamineManager.Instance.BuildingEmptyIndexOnStartup -= OnInstanceOnBuildingEmptyIndexOnStartup;

            //don't do anything if we have disabled this
            if (_disableExamineIndexing == false)
            {
                foreach (var indexer in _indexesToRebuild)
                {
                    indexer.RebuildIndex();
                }
            }
        }

        /// <summary>
        /// Called to perform the rebuilding indexes on startup if the indexes don't exist
        /// </summary>
        public void RebuildIndexes()
        {
            //don't do anything if we have disabled this
            if (_disableExamineIndexing) return;

            EnsureUnlockedAndConfigured();

            //If the developer has explicitly opted out of rebuilding indexes on startup then we
            // should adhere to that and not do it, this means that if they are load balancing things will be
            // out of sync if they are auto-scaling but there's not much we can do about that.
            if (ExamineSettings.Instance.RebuildOnAppStart == false) return;

            foreach (var indexer in GetIndexesForColdBoot())
            {
                indexer.RebuildIndex();
            }
        }

        /// <summary>
        /// The method used to create indexes on a cold boot
        /// </summary>
        /// <remarks>
        /// A cold boot is when the server determines it will not (or cannot) process instructions in the cache table and
        /// will rebuild it's own caches itself.
        /// </remarks>
        public IEnumerable<BaseIndexProvider> GetIndexesForColdBoot()
        {
            // NOTE: This is IMPORTANT! ... we don't want to rebuild any index that is already flagged to be re-indexed
            // on startup based on our _indexesToRebuild variable and how Examine auto-rebuilds when indexes are empty.
            // This callback is used above for the DatabaseServerMessenger startup options.

            // all indexes
            IEnumerable<BaseIndexProvider> indexes = ExamineManager.Instance.IndexProviderCollection;

            // except those that are already flagged
            // and are processed in Complete()
            if (_indexesToRebuild.Any())
                indexes = indexes.Except(_indexesToRebuild);

            // return
            foreach (var index in indexes)
                yield return index;
        }

        /// <summary>
        /// Must be called to configure each index and ensure it's unlocked before any indexing occurs
        /// </summary>
        /// <remarks>
        /// Indexing rebuilding can occur on a normal boot if the indexes are empty or on a cold boot by the database server messenger. Before
        /// either of these happens, we need to configure the indexes.
        /// </remarks>
        private void EnsureUnlockedAndConfigured()
        {
            if (_isConfigured) return;

            _isConfigured = true;

            foreach (var luceneIndexer in ExamineManager.Instance.IndexProviderCollection.OfType<LuceneIndexer>())
            {
                //We now need to disable waiting for indexing for Examine so that the appdomain is shutdown immediately and doesn't wait for pending
                //indexing operations. We used to wait for indexing operations to complete but this can cause more problems than that is worth because
                //that could end up halting shutdown for a very long time causing overlapping appdomains and many other problems.
                luceneIndexer.WaitForIndexQueueOnShutdown = false;

                //we should check if the index is locked ... it shouldn't be! We are using simple fs lock now and we are also ensuring that
                //the indexes are not operational unless MainDom is true so if _disableExamineIndexing is false then we should be in charge
                if (_disableExamineIndexing == false)
                {
                    var dir = luceneIndexer.GetLuceneDirectory();
                    if (IndexWriter.IsLocked(dir))
                    {
                        _profilingLogger.Logger.Info<ExamineStartup>("Forcing index " + luceneIndexer.IndexSetName + " to be unlocked since it was left in a locked state");
                        IndexWriter.Unlock(dir);
                    }
                }
            }
        }

        private void OnInstanceOnBuildingEmptyIndexOnStartup(object sender, BuildingEmptyIndexOnStartupEventArgs args)
        {
            //store the indexer that needs rebuilding because it's empty for when the boot process
            // is complete and cancel this current event so the rebuild process doesn't start right now.
            args.Cancel = true;
            _indexesToRebuild.Add((BaseIndexProvider)args.Indexer);

            //check if the index is rebuilding due to an error and log it
            if (args.IsHealthy == false)
            {
                var baseIndex = args.Indexer as BaseIndexProvider;
                var name = baseIndex != null ? baseIndex.Name : "[UKNOWN]";

                _profilingLogger.Logger.Error<ExamineStartup>(string.Format("The index {0} is rebuilding due to being unreadable/corrupt", name), args.UnhealthyException);
            }
        }
    }
}