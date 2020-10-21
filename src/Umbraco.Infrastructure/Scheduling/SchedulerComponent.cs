using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Scheduling
{
    public sealed class SchedulerComponent : IComponent
    {
        private const int DefaultDelayMilliseconds = 180000; // 3 mins
        private const int OneMinuteMilliseconds = 60000;
        private const int FiveMinuteMilliseconds = 300000;
        private const int OneHourMilliseconds = 3600000;

        private readonly IRuntimeState _runtime;
        private readonly IMainDom _mainDom;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IContentService _contentService;
        private readonly IAuditService _auditService;
        private readonly IProfilingLogger _logger;
        private readonly IApplicationShutdownRegistry _hostingEnvironment;
        private readonly IScopeProvider _scopeProvider;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IHealthChecksSettings _healthChecksSettingsConfig;
        private readonly IIOHelper _ioHelper;
        private readonly IServerMessenger _serverMessenger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly ILoggingSettings _loggingSettings;
        private readonly IKeepAliveSettings _keepAliveSettings;

        private BackgroundTaskRunner<IBackgroundTask> _keepAliveRunner;
        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;
        private BackgroundTaskRunner<IBackgroundTask> _fileCleanupRunner;
        private BackgroundTaskRunner<IBackgroundTask> _healthCheckRunner;

        private bool _started;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;

        public SchedulerComponent(IRuntimeState runtime, IMainDom mainDom, IServerRegistrar serverRegistrar,
            IContentService contentService, IAuditService auditService,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IScopeProvider scopeProvider, IUmbracoContextFactory umbracoContextFactory, IProfilingLogger logger,
            IApplicationShutdownRegistry hostingEnvironment, IHealthChecksSettings healthChecksSettingsConfig,
            IIOHelper ioHelper, IServerMessenger serverMessenger, IRequestAccessor requestAccessor,
            ILoggingSettings loggingSettings, IKeepAliveSettings keepAliveSettings)
        {
            _runtime = runtime;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _contentService = contentService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _umbracoContextFactory = umbracoContextFactory;

            _healthChecks = healthChecks;
            _notifications = notifications;
            _healthChecksSettingsConfig = healthChecksSettingsConfig ?? throw new ArgumentNullException(nameof(healthChecksSettingsConfig));
            _ioHelper = ioHelper;
            _serverMessenger = serverMessenger;
            _requestAccessor = requestAccessor;
            _loggingSettings = loggingSettings;
            _keepAliveSettings = keepAliveSettings;
        }

        public void Initialize()
        {
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", _logger, _hostingEnvironment);
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", _logger, _hostingEnvironment);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", _logger, _hostingEnvironment);
            _fileCleanupRunner = new BackgroundTaskRunner<IBackgroundTask>("TempFileCleanup", _logger, _hostingEnvironment);
            _healthCheckRunner = new BackgroundTaskRunner<IBackgroundTask>("HealthCheckNotifier", _logger, _hostingEnvironment);

            // we will start the whole process when a successful request is made
            _requestAccessor.RouteAttempt += RegisterBackgroundTasksOnce;
        }

        public void Terminate()
        {
            // the AppDomain / maindom / whatever takes care of stopping background task runners
        }

        private void RegisterBackgroundTasksOnce(object sender, RoutableAttemptEventArgs e)
        {
            switch (e.Outcome)
            {
                case EnsureRoutableOutcome.IsRoutable:
                case EnsureRoutableOutcome.NotDocumentRequest:
                    _requestAccessor.RouteAttempt -= RegisterBackgroundTasksOnce;
                    RegisterBackgroundTasks();
                    break;
            }
        }

        private void RegisterBackgroundTasks()
        {
            LazyInitializer.EnsureInitialized(ref _tasks, ref _started, ref _locker, () =>
            {
                _logger.Debug<SchedulerComponent>("Initializing the scheduler");

                var tasks = new List<IBackgroundTask>();

                if (_keepAliveSettings.DisableKeepAliveTask == false)
                {
                    tasks.Add(RegisterKeepAlive(_keepAliveSettings));
                }

                tasks.Add(RegisterScheduledPublishing());
                tasks.Add(RegisterLogScrubber(_loggingSettings));
                tasks.Add(RegisterTempFileCleanup());

                var healthCheckConfig = _healthChecksSettingsConfig;
                if (healthCheckConfig.NotificationSettings.Enabled)
                    tasks.Add(RegisterHealthCheckNotifier(healthCheckConfig, _healthChecks, _notifications, _logger));

                return tasks.ToArray();
            });
        }

        private IBackgroundTask RegisterKeepAlive(IKeepAliveSettings keepAliveSettings)
        {
            // ping/keepalive
            // on all servers
            var task = new KeepAlive(_keepAliveRunner, DefaultDelayMilliseconds, FiveMinuteMilliseconds, _runtime, _mainDom, keepAliveSettings, _logger, _serverRegistrar);
            _keepAliveRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterScheduledPublishing()
        {
            // scheduled publishing/unpublishing
            // install on all, will only run on non-replica servers
            var task = new ScheduledPublishing(_publishingRunner, DefaultDelayMilliseconds, OneMinuteMilliseconds, _runtime, _mainDom, _serverRegistrar, _contentService, _umbracoContextFactory, _logger, _serverMessenger);
            _publishingRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterHealthCheckNotifier(IHealthChecksSettings healthCheckSettingsConfig,
            HealthCheckCollection healthChecks, HealthCheckNotificationMethodCollection notifications,
            IProfilingLogger logger)
        {
            // If first run time not set, start with just small delay after application start
            int delayInMilliseconds;
            if (string.IsNullOrEmpty(healthCheckSettingsConfig.NotificationSettings.FirstRunTime))
            {
                delayInMilliseconds = DefaultDelayMilliseconds;
            }
            else
            {
                // Otherwise start at scheduled time
                delayInMilliseconds = DateTime.Now.PeriodicMinutesFrom(healthCheckSettingsConfig.NotificationSettings.FirstRunTime) * 60 * 1000;
                if (delayInMilliseconds < DefaultDelayMilliseconds)
                {
                    delayInMilliseconds = DefaultDelayMilliseconds;
                }
            }

            var periodInMilliseconds = healthCheckSettingsConfig.NotificationSettings.PeriodInHours * 60 * 60 * 1000;
            var task = new HealthCheckNotifier(_healthCheckRunner, delayInMilliseconds, periodInMilliseconds, healthChecks, notifications, _mainDom, logger, _healthChecksSettingsConfig, _serverRegistrar, _runtime, _scopeProvider);
            _healthCheckRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterLogScrubber(ILoggingSettings settings)
        {
            // log scrubbing
            // install on all, will only run on non-replica servers
            var task = new LogScrubber(_scrubberRunner, DefaultDelayMilliseconds, LogScrubber.GetLogScrubbingInterval(), _mainDom, _serverRegistrar, _auditService, settings, _scopeProvider, _logger);
            _scrubberRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterTempFileCleanup()
        {
            // temp file cleanup, will run on all servers - even though file upload should only be handled on the master, this will
            // ensure that in the case it happes on replicas that they are cleaned up.
            var task = new TempFileCleanup(_fileCleanupRunner, DefaultDelayMilliseconds, OneHourMilliseconds,
                new[] { new DirectoryInfo(_ioHelper.MapPath(Constants.SystemDirectories.TempFileUploads)) },
                TimeSpan.FromDays(1), //files that are over a day old
                _mainDom, _logger);
            _scrubberRunner.TryAdd(task);
            return task;
        }
    }
}
