using System;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Examine;
using Umbraco.Web.Routing;
using Umbraco.Web.Scheduling;

namespace Umbraco.Web.Compose
{

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

        public DatabaseServerRegistrarAndMessengerComponent(
            IRuntimeState runtime,
            IServerRegistrar serverRegistrar,
            IServerMessenger serverMessenger,
            IServerRegistrationService registrationService,
            ILogger logger)
        {
            _runtime = runtime;
            _logger = logger;
            _registrationService = registrationService;

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
            {
                UmbracoModule.RouteAttempt += RegisterBackgroundTasksOnce;
                UmbracoModule.EndRequest += UmbracoModule_EndRequest;
            }
        }

        public void Terminate()
        { }

        private void UmbracoModule_EndRequest(object sender, UmbracoRequestEventArgs e)
        {
            // will clear the batch - will remain in HttpContext though - that's ok
            _messenger?.FlushBatch();
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
                    _logger.Error<InstructionProcessTask>(e, "Failed (will repeat).");
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
