using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Used to do the scheduling for tasks, publishing, etc...
    /// </summary>
    /// <remarks>
    /// All tasks are run in a background task runner which is web aware and will wind down
    /// the task correctly instead of killing it completely when the app domain shuts down.
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    internal sealed class SchedulerComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        private IRuntimeState _runtime;
        private IUserService _userService;
        private IAuditService _auditService;
        private ILogger _logger;
        private ProfilingLogger _proflog;
        private IScopeProvider _scopeProvider;

        private BackgroundTaskRunner<IBackgroundTask> _keepAliveRunner;
        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _tasksRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;

        private bool _started;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;

        public void Initialize(IRuntimeState runtime, IUserService userService, IAuditService auditService, IScopeProvider scopeProvider, ILogger logger, ProfilingLogger proflog)
        {
            _runtime = runtime;
            _userService = userService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _proflog = proflog;

            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", logger);
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", logger);
            _tasksRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledTasks", logger);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", logger);

            // we will start the whole process when a successful request is made
            UmbracoModule.RouteAttempt += UmbracoModuleRouteAttempt;
        }

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

            LazyInitializer.EnsureInitialized(ref _tasks, ref _started, ref _locker, () =>
            {
                _logger.Debug<SchedulerComponent>(() => "Initializing the scheduler");
                var settings = UmbracoConfig.For.UmbracoSettings();

                var tasks = new List<IBackgroundTask>
                {
                    new KeepAlive(_keepAliveRunner, 60000, 300000, _runtime, _logger, _proflog),
                    new ScheduledPublishing(_publishingRunner, 60000, 60000, _runtime, _userService, _scopeProvider, _logger, _proflog),
                    new ScheduledTasks(_tasksRunner, 60000, 60000, _runtime, settings, _logger, _proflog),
                    new LogScrubber(_scrubberRunner, 60000, LogScrubber.GetLogScrubbingInterval(settings, _logger), _runtime, _auditService, settings, _scopeProvider, _logger, _proflog)
                };

                // ping/keepalive
                // on all servers
                _keepAliveRunner.TryAdd(tasks[0]);

                // scheduled publishing/unpublishing
                // install on all, will only run on non-slaves servers
                _publishingRunner.TryAdd(tasks[1]);

                _tasksRunner.TryAdd(tasks[2]);

                // log scrubbing
                // install on all, will only run on non-slaves servers
                _scrubberRunner.TryAdd(tasks[3]);

                return tasks.ToArray();
            });
        }
    }
}
