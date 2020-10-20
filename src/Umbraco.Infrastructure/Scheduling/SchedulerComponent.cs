using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
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
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<SchedulerComponent> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IApplicationShutdownRegistry _applicationShutdownRegistry;
        private readonly IScopeProvider _scopeProvider;
        private readonly HealthCheckCollection _healthChecks;
        private readonly HealthCheckNotificationMethodCollection _notifications;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly HealthChecksSettings _healthChecksSettings;
        private readonly IServerMessenger _serverMessenger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IBackofficeSecurityFactory _backofficeSecurityFactory;
        private readonly LoggingSettings _loggingSettings;
        private readonly KeepAliveSettings _keepAliveSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

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
            IScopeProvider scopeProvider, IUmbracoContextFactory umbracoContextFactory, IProfilingLogger profilingLogger , ILoggerFactory loggerFactory,
            IApplicationShutdownRegistry applicationShutdownRegistry, IOptions<HealthChecksSettings> healthChecksSettings,
            IServerMessenger serverMessenger, IRequestAccessor requestAccessor,
            IOptions<LoggingSettings> loggingSettings, IOptions<KeepAliveSettings> keepAliveSettings,
            IHostingEnvironment hostingEnvironment,
            IBackofficeSecurityFactory backofficeSecurityFactory)
        {
            _runtime = runtime;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _contentService = contentService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _profilingLogger = profilingLogger ;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SchedulerComponent>();
            _applicationShutdownRegistry = applicationShutdownRegistry;
            _umbracoContextFactory = umbracoContextFactory;

            _healthChecks = healthChecks;
            _notifications = notifications;
            _healthChecksSettings = healthChecksSettings.Value ?? throw new ArgumentNullException(nameof(healthChecksSettings));
            _serverMessenger = serverMessenger;
            _requestAccessor = requestAccessor;
            _backofficeSecurityFactory = backofficeSecurityFactory;
            _loggingSettings = loggingSettings.Value;
            _keepAliveSettings = keepAliveSettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Initialize()
        {
            var logger = _loggerFactory.CreateLogger<BackgroundTaskRunner<IBackgroundTask>>();
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _keepAliveRunner = new BackgroundTaskRunner<IBackgroundTask>("KeepAlive", logger, _applicationShutdownRegistry);
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", logger, _applicationShutdownRegistry);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", logger, _applicationShutdownRegistry);
            _fileCleanupRunner = new BackgroundTaskRunner<IBackgroundTask>("TempFileCleanup", logger, _applicationShutdownRegistry);
            _healthCheckRunner = new BackgroundTaskRunner<IBackgroundTask>("HealthCheckNotifier", logger, _applicationShutdownRegistry);

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
                _logger.LogDebug("Initializing the scheduler");

                var tasks = new List<IBackgroundTask>();

                if (_keepAliveSettings.DisableKeepAliveTask == false)
                {
                    tasks.Add(RegisterKeepAlive(_keepAliveSettings));
                }

                tasks.Add(RegisterScheduledPublishing());
                tasks.Add(RegisterLogScrubber(_loggingSettings));
                tasks.Add(RegisterTempFileCleanup());

                var healthCheckConfig = _healthChecksSettings;
                if (healthCheckConfig.NotificationSettings.Enabled)
                    tasks.Add(RegisterHealthCheckNotifier(healthCheckConfig, _healthChecks, _notifications, _profilingLogger));

                return tasks.ToArray();
            });
        }

        private IBackgroundTask RegisterKeepAlive(KeepAliveSettings keepAliveSettings)
        {
            // ping/keepalive
            // on all servers
            var task = new KeepAlive(_keepAliveRunner, DefaultDelayMilliseconds, FiveMinuteMilliseconds, _requestAccessor, _mainDom, Options.Create(keepAliveSettings), _loggerFactory.CreateLogger<KeepAlive>(), _profilingLogger, _serverRegistrar);
            _keepAliveRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterScheduledPublishing()
        {
            // scheduled publishing/unpublishing
            // install on all, will only run on non-replica servers
            var task = new ScheduledPublishing(_publishingRunner, DefaultDelayMilliseconds, OneMinuteMilliseconds, _runtime, _mainDom, _serverRegistrar, _contentService, _umbracoContextFactory, _loggerFactory.CreateLogger<ScheduledPublishing>(), _serverMessenger, _backofficeSecurityFactory);
            _publishingRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterHealthCheckNotifier(HealthChecksSettings healthCheckSettingsConfig,
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
            var task = new HealthCheckNotifier(_healthCheckRunner, delayInMilliseconds, periodInMilliseconds, healthChecks, notifications, _mainDom, logger, _loggerFactory.CreateLogger<HealthCheckNotifier>(), _healthChecksSettings, _serverRegistrar, _runtime, _scopeProvider);
            _healthCheckRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterLogScrubber(LoggingSettings settings)
        {
            // log scrubbing
            // install on all, will only run on non-replica servers
            var task = new LogScrubber(_scrubberRunner, DefaultDelayMilliseconds, LogScrubber.GetLogScrubbingInterval(), _mainDom, _serverRegistrar, _auditService, Options.Create(settings), _scopeProvider, _profilingLogger, _loggerFactory.CreateLogger<LogScrubber>());
            _scrubberRunner.TryAdd(task);
            return task;
        }

        private IBackgroundTask RegisterTempFileCleanup()
        {

            var tempFolderPaths = new[]
            {
                _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads)
            };

            foreach (var tempFolderPath in tempFolderPaths)
            {
                //ensure it exists
                Directory.CreateDirectory(tempFolderPath);
            }

            // temp file cleanup, will run on all servers - even though file upload should only be handled on the master, this will
            // ensure that in the case it happes on replicas that they are cleaned up.
            var task = new TempFileCleanup(_fileCleanupRunner, DefaultDelayMilliseconds, OneHourMilliseconds,
                tempFolderPaths.Select(x=>new DirectoryInfo(x)),
                TimeSpan.FromDays(1), //files that are over a day old
                _mainDom, _profilingLogger, _loggerFactory.CreateLogger<TempFileCleanup>());
            _scrubberRunner.TryAdd(task);
            return task;
        }
    }
}
