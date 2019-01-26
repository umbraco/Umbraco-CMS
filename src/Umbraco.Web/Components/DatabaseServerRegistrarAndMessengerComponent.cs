using System;
using System.Threading;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Changes;
using Umbraco.Core.Sync;
using Umbraco.Examine;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;
using Umbraco.Web.Scheduling;
using Umbraco.Web.Search;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Components
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

    // during Initialize / Startup, we end up checking Examine, which needs to be initialized beforehand
    // todo - should not be a strong dependency on "examine" but on an "indexing component"
    [ComposeAfter(typeof(ExamineComposer))]

    public sealed class DatabaseServerRegistrarAndMessengerComposer : ComponentComposer<DatabaseServerRegistrarAndMessengerComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.SetServerMessenger(factory =>
            {
                var runtime = factory.GetInstance<IRuntimeState>();
                var databaseFactory = factory.GetInstance<IUmbracoDatabaseFactory>();
                var globalSettings = factory.GetInstance<IGlobalSettings>();
                var proflog = factory.GetInstance<IProfilingLogger>();
                var scopeProvider = factory.GetInstance<IScopeProvider>();
                var sqlContext = factory.GetInstance<ISqlContext>();
                var logger = factory.GetInstance<ILogger>();
                var indexRebuilder = factory.GetInstance<IndexRebuilder>();

                return new BatchedDatabaseServerMessenger(
                    runtime, databaseFactory, scopeProvider, sqlContext, proflog, globalSettings,
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
                                svc.Notify(new[] { new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll) });
                                svc.Notify(new[] { new ContentCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out _, out _);
                                svc.Notify(new[] { new MediaCacheRefresher.JsonPayload(0, TreeChangeTypes.RefreshAll) }, out _);
                            },

                            //rebuild indexes if the server is not synced
                            // NOTE: This will rebuild ALL indexes including the members, if developers want to target specific
                            // indexes then they can adjust this logic themselves.
                            () =>
                            {
                                ExamineComponent.RebuildIndexes(indexRebuilder, logger, false, 5000);
                            }
                        }
                    });
            });
        }
    }

    public sealed class DatabaseServerRegistrarAndMessengerComponent : IComponent
    {
        private object _locker = new object();
        private readonly DatabaseServerRegistrar _registrar;
        private readonly BatchedDatabaseServerMessenger _messenger;
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;
        private readonly IServerRegistrationService _registrationService;
        private readonly BackgroundTaskRunner<IBackgroundTask> _touchTaskRunner;
        private readonly BackgroundTaskRunner<IBackgroundTask> _processTaskRunner;
        private bool _started;
        private IBackgroundTask[] _tasks;
        private IndexRebuilder _indexRebuilder;

        public DatabaseServerRegistrarAndMessengerComponent(IRuntimeState runtime, IServerRegistrar serverRegistrar, IServerMessenger serverMessenger, IServerRegistrationService registrationService, ILogger logger, IndexRebuilder indexRebuilder)
        {
            _runtime = runtime;
            _logger = logger;
            _registrationService = registrationService;
            _indexRebuilder = indexRebuilder;

            // create task runner for DatabaseServerRegistrar
            _registrar = serverRegistrar as DatabaseServerRegistrar;
            if (_registrar != null)
            {
                _touchTaskRunner = new BackgroundTaskRunner<IBackgroundTask>("ServerRegistration",
                    new BackgroundTaskRunnerOptions { AutoStart = true }, logger);
            }

            // create task runner for BatchedDatabaseServerMessenger
            _messenger = serverMessenger as BatchedDatabaseServerMessenger;
            if (_messenger != null)
            {
                _processTaskRunner = new BackgroundTaskRunner<IBackgroundTask>("ServerInstProcess",
                    new BackgroundTaskRunnerOptions { AutoStart = true }, logger);
            }
        }

        public void Initialize()
        { 
            //We will start the whole process when a successful request is made
            if (_registrar != null || _messenger != null)
                UmbracoModule.RouteAttempt += RegisterBackgroundTasksOnce;

            // must come last, as it references some _variables
            _messenger?.Startup();
        }

        public void Terminate()
        { }

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
        private void RegisterBackgroundTasksOnce(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                    UmbracoModule.RouteAttempt -= RegisterBackgroundTasksOnce;
                    RegisterBackgroundTasks();
                    break;
            }
        }

        private void RegisterBackgroundTasks()
        {
            // only perform this one time ever
            LazyInitializer.EnsureInitialized(ref _tasks, ref _started, ref _locker, () =>
            {
                var serverAddress = _runtime.ApplicationUrl.ToString();

                return new[]
                {
                    RegisterInstructionProcess(),
                    RegisterTouchServer(_registrationService, serverAddress)
                };
            });
        }

        private IBackgroundTask RegisterInstructionProcess()
        {
            if (_messenger == null)
                return null;

            var task = new InstructionProcessTask(_processTaskRunner,
                60000, //delay before first execution
                _messenger.Options.ThrottleSeconds*1000, //amount of ms between executions
                _messenger,
                _logger);
            _processTaskRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterTouchServer(IServerRegistrationService registrationService, string serverAddress)
        {
            if (_registrar == null)
                return null;

            var task = new TouchServerTask(_touchTaskRunner,
                15000, //delay before first execution
                _registrar.Options.RecurringSeconds*1000, //amount of ms between executions
                registrationService, _registrar, serverAddress, _logger);
            _touchTaskRunner.TryAdd(task);
            return task;
        }

        private class InstructionProcessTask : RecurringTaskBase
        {
            private readonly DatabaseServerMessenger _messenger;
            private readonly ILogger _logger;

            public InstructionProcessTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
                DatabaseServerMessenger messenger, ILogger logger)
                : base(runner, delayMilliseconds, periodMilliseconds)
            {
                _messenger = messenger;
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
                    _messenger.Sync();
                }
                catch (Exception e)
                {
                    _logger.Error<InstructionProcessTask>("Failed (will repeat).", e);
                }
                return true; // repeat
            }
        }

        private class TouchServerTask : RecurringTaskBase
        {
            private readonly IServerRegistrationService _svc;
            private readonly DatabaseServerRegistrar _registrar;
            private readonly string _serverAddress;
            private readonly ILogger _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="TouchServerTask"/> class.
            /// </summary>
            public TouchServerTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
                IServerRegistrationService svc, DatabaseServerRegistrar registrar, string serverAddress, ILogger logger)
                : base(runner, delayMilliseconds, periodMilliseconds)
            {
                _svc = svc ?? throw new ArgumentNullException(nameof(svc));
                _registrar = registrar;
                _serverAddress = serverAddress;
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
                    _logger.Error<DatabaseServerRegistrarAndMessengerComponent>(ex, "Failed to update server record in database.");
                    return false; // probably stop if we have an error
                }
            }
        }
    }
}
