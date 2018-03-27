using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;
using Umbraco.Web.Scheduling;
using LightInject;
using Umbraco.Core.Exceptions;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Ensures that servers are automatically registered in the database, when using the database server registrar.
    /// </summary>
    /// <remarks>
    /// <para>At the moment servers are automatically registered upon first request and then on every
    /// request but not more than once per (configurable) period. This really is "for information & debug" purposes so
    /// we can look at the table and see what servers are registered - but the info is not used anywhere.</para>
    /// <para>Should we actually want to use this, we would need a better and more deterministic way of figuring
    /// out the "server address" ie the address to which server-to-server requests should be sent - because it
    /// probably is not the "current request address" - especially in multi-domains configurations.</para>
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class DatabaseServerRegistrarAndMessengerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private object _locker = new object();
        private DatabaseServerRegistrar _registrar;
        private IRuntimeState _runtime;
        private ILogger _logger;
        private IServerRegistrationService _registrationService;
        private BackgroundTaskRunner<IBackgroundTask> _backgroundTaskRunner;
        private bool _started;
        private TouchServerTask _task;
        private IUmbracoDatabaseFactory _databaseFactory;

        public override void Compose(Composition composition)
        {
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled) return;

            composition.SetServerMessenger(factory =>
            {
                var runtime = factory.GetInstance<IRuntimeState>();
                var databaseFactory = factory.GetInstance<IUmbracoDatabaseFactory>();
                var logger = factory.GetInstance<ILogger>();
                var proflog = factory.GetInstance<ProfilingLogger>();
                var scopeProvider = factory.GetInstance<IScopeProvider>();
                var sqlContext = factory.GetInstance<ISqlContext>();

                return new BatchedDatabaseServerMessenger(
                    runtime, databaseFactory, scopeProvider, sqlContext, logger, proflog,
                    true,
                    //Default options for web including the required callbacks to build caches
                    new DatabaseServerMessengerOptions
                    {
                        //These callbacks will be executed if the server has not been synced
                        // (i.e. it is a new server or the lastsynced.txt file has been removed)
                        InitializingCallbacks = new Action[]
                        {
                            //rebuild the xml cache file if the server is not synced
                            () =>
                            {
                                // rebuild the published snapshot caches entirely, if the server is not synced
                                // this is equivalent to DistributedCache RefreshAll... but local only
                                // (we really should have a way to reuse RefreshAll... locally)
                                // note: refresh all content & media caches does refresh content types too
                                var svc = Current.PublishedSnapshotService;
                                bool ignored1, ignored2;
                                svc.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                                svc.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1, out ignored2);
                                svc.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out ignored1);
                            },

                            //rebuild indexes if the server is not synced
                            // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                            // indexes then they can adjust this logic themselves.
                            () => RebuildIndexes(false)
                        }
                    });
            });
        }

        // fixme - this should move to something else, we should not depend on Examine here!
        private static void RebuildIndexes(bool onlyEmptyIndexes)
        {
            var indexers = (IEnumerable<KeyValuePair<string, IIndexer>>) ExamineManager.Instance.IndexProviders;
            if (onlyEmptyIndexes)
                indexers = indexers.Where(x => x.Value.IsIndexNew());
            foreach (var indexer in indexers)
                indexer.Value.RebuildIndex();
        }

        public void Initialize(IRuntimeState runtime, IServerRegistrar serverRegistrar, IServerRegistrationService registrationService, IUmbracoDatabaseFactory databaseFactory, ILogger logger)
        {
            if (UmbracoConfig.For.UmbracoSettings().DistributedCall.Enabled) return;

            _registrar = serverRegistrar as DatabaseServerRegistrar;
            if (_registrar == null) throw new Exception("panic: registar.");

            _runtime = runtime;
            _databaseFactory = databaseFactory;
            _logger = logger;
            _registrationService = registrationService;

            _backgroundTaskRunner = new BackgroundTaskRunner<IBackgroundTask>(
                new BackgroundTaskRunnerOptions { AutoStart = true },
                logger);

            //We will start the whole process when a successful request is made
            UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }

        /// <summary>
        /// Handle when a request is made
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// We require this because:
        /// - ApplicationContext.UmbracoApplicationUrl is initialized by UmbracoModule in BeginRequest
        /// - RegisterServer is called on UmbracoModule.RouteAttempt which is triggered in ProcessRequest
        ///      we are safe, UmbracoApplicationUrl has been initialized
        /// </remarks>
        private void UmbracoModuleRouteAttempt(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                    RegisterBackgroundTasks(e);
                    break;
            }
        }

        private void RegisterBackgroundTasks(UmbracoRequestEventArgs e)
        {
            // remove handler, we're done
            UmbracoModule.RouteAttempt -= UmbracoModuleRouteAttempt;

            // only perform this one time ever
            LazyInitializer.EnsureInitialized(ref _task, ref _started, ref _locker, () =>
            {
                var serverAddress = _runtime.ApplicationUrl.ToString();
                var svc = _registrationService;

                var task = new TouchServerTask(_backgroundTaskRunner,
                    15000, //delay before first execution
                    _registrar.Options.RecurringSeconds*1000, //amount of ms between executions
                    svc, _registrar, serverAddress, _databaseFactory, _logger);

                // perform the rest async, we don't want to block the startup sequence
                // this will just reoccur on a background thread
                _backgroundTaskRunner.TryAdd(task);

                return task;
            });
        }

        private class TouchServerTask : RecurringTaskBase
        {
            private readonly IServerRegistrationService _svc;
            private readonly DatabaseServerRegistrar _registrar;
            private readonly string _serverAddress;
            private readonly IUmbracoDatabaseFactory _databaseFactory;
            private readonly ILogger _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="RecurringTaskBase"/> class.
            /// </summary>
            /// <param name="runner">The task runner.</param>
            /// <param name="delayMilliseconds">The delay.</param>
            /// <param name="periodMilliseconds">The period.</param>
            /// <param name="svc"></param>
            /// <param name="registrar"></param>
            /// <param name="serverAddress"></param>
            /// <param name="databaseContext"></param>
            /// <param name="logger"></param>
            /// <remarks>The task will repeat itself periodically. Use this constructor to create a new task.</remarks>
            public TouchServerTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
                IServerRegistrationService svc, DatabaseServerRegistrar registrar, string serverAddress, IUmbracoDatabaseFactory databaseFactory, ILogger logger)
                : base(runner, delayMilliseconds, periodMilliseconds)
            {
                if (svc == null) throw new ArgumentNullException(nameof(svc));
                _svc = svc;
                _registrar = registrar;
                _serverAddress = serverAddress;
                _databaseFactory = databaseFactory;
                _logger = logger;
            }

            public override bool IsAsync => false;

            /// <summary>
            /// Runs the background task.
            /// </summary>
            /// <returns>A value indicating whether to repeat the task.</returns>
            public override bool PerformRun()
            {
                try
                {
                    // TouchServer uses a proper unit of work etc underneath so even in a
                    // background task it is safe to call it without dealing with any scope
                    _svc.TouchServer(_serverAddress, _svc.CurrentServerIdentity, _registrar.Options.StaleServerTimeout);
                    return true; // repeat
                }
                catch (Exception ex)
                {
                    _logger.Error<DatabaseServerRegistrarAndMessengerComponent>("Failed to update server record in database.", ex);
                    return false; // probably stop if we have an error
                }
            }
        }
    }
}
