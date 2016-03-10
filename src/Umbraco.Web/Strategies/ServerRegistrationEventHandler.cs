using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;
using Umbraco.Web.Scheduling;

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
    public sealed class ServerRegistrationEventHandler : ApplicationEventHandler
    {
        private DatabaseServerRegistrar _registrar;
        private BackgroundTaskRunner<IBackgroundTask> _backgroundTaskRunner;
        private bool _started = false;
        private TouchServerTask _task;
        private object _lock = new object();

        // bind to events
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _registrar = ServerRegistrarResolver.Current.Registrar as DatabaseServerRegistrar;
            
            // only for the DatabaseServerRegistrar
            if (_registrar == null) return;

            _backgroundTaskRunner = new BackgroundTaskRunner<IBackgroundTask>(
                new BackgroundTaskRunnerOptions { AutoStart = true },
                applicationContext.ProfilingLogger.Logger);

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
            //remove handler, we're done
            UmbracoModule.RouteAttempt -= UmbracoModuleRouteAttempt;

            //only perform this one time ever
            LazyInitializer.EnsureInitialized(ref _task, ref _started, ref _lock, () =>
            {
                var serverAddress = e.UmbracoContext.Application.UmbracoApplicationUrl;
                var svc = e.UmbracoContext.Application.Services.ServerRegistrationService;

                var task = new TouchServerTask(_backgroundTaskRunner,
                    15000, //delay before first execution
                    _registrar.Options.RecurringSeconds*1000, //amount of ms between executions
                    svc, _registrar, serverAddress);
                
                //Perform the rest async, we don't want to block the startup sequence
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

            /// <summary>
            /// Initializes a new instance of the <see cref="RecurringTaskBase"/> class.
            /// </summary>
            /// <param name="runner">The task runner.</param>
            /// <param name="delayMilliseconds">The delay.</param>
            /// <param name="periodMilliseconds">The period.</param>
            /// <param name="svc"></param>
            /// <param name="registrar"></param>
            /// <param name="serverAddress"></param>
            /// <remarks>The task will repeat itself periodically. Use this constructor to create a new task.</remarks>
            public TouchServerTask(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
                IServerRegistrationService svc, DatabaseServerRegistrar registrar, string serverAddress)
                : base(runner, delayMilliseconds, periodMilliseconds)
            {
                if (svc == null) throw new ArgumentNullException("svc");
                _svc = svc;
                _registrar = registrar;
                _serverAddress = serverAddress;
            }

            public override bool IsAsync
            {
                get { return false; }
            }

            public override bool RunsOnShutdown
            {
                get { return false; }
            }

            /// <summary>
            /// Runs the background task.
            /// </summary>
            /// <returns>A value indicating whether to repeat the task.</returns>
            public override bool PerformRun()
            {
                try
                {
                    _svc.TouchServer(_serverAddress, _svc.CurrentServerIdentity, _registrar.Options.StaleServerTimeout);

                    return true; // repeat
                }
                catch (Exception ex)
                {
                    LogHelper.Error<ServerRegistrationEventHandler>("Failed to update server record in database.", ex);

                    return false; // probably stop if we have an error
                }
            }

            public override Task<bool> PerformRunAsync(CancellationToken token)
            {
                throw new NotImplementedException();
            }
        }
    }
}
