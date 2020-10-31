using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Scheduling
{
    public sealed class SchedulerComponent : IComponent
    {
        private const int DefaultDelayMilliseconds = 180000; // 3 mins
        private const int OneMinuteMilliseconds = 60000;

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
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IServerMessenger _serverMessenger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IBackofficeSecurityFactory _backofficeSecurityFactory;
        private readonly LoggingSettings _loggingSettings;
        private readonly IHostingEnvironment _hostingEnvironment;

        private BackgroundTaskRunner<IBackgroundTask> _publishingRunner;
        private BackgroundTaskRunner<IBackgroundTask> _scrubberRunner;

        private bool _started;
        private object _locker = new object();
        private IBackgroundTask[] _tasks;

        public SchedulerComponent(IRuntimeState runtime, IMainDom mainDom, IServerRegistrar serverRegistrar,
            IContentService contentService, IAuditService auditService,
            IScopeProvider scopeProvider, IUmbracoContextFactory umbracoContextFactory, IProfilingLogger profilingLogger, ILoggerFactory loggerFactory,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IServerMessenger serverMessenger, IRequestAccessor requestAccessor,
            IOptions<LoggingSettings> loggingSettings,
            IHostingEnvironment hostingEnvironment,
            IBackofficeSecurityFactory backofficeSecurityFactory)
        {
            _runtime = runtime;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _contentService = contentService;
            _auditService = auditService;
            _scopeProvider = scopeProvider;
            _profilingLogger = profilingLogger;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SchedulerComponent>();
            _applicationShutdownRegistry = applicationShutdownRegistry;
            _umbracoContextFactory = umbracoContextFactory;
            _serverMessenger = serverMessenger;
            _requestAccessor = requestAccessor;
            _backofficeSecurityFactory = backofficeSecurityFactory;
            _loggingSettings = loggingSettings.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Initialize()
        {
            var logger = _loggerFactory.CreateLogger<BackgroundTaskRunner<IBackgroundTask>>();
            // backgrounds runners are web aware, if the app domain dies, these tasks will wind down correctly
            _publishingRunner = new BackgroundTaskRunner<IBackgroundTask>("ScheduledPublishing", logger, _applicationShutdownRegistry);
            _scrubberRunner = new BackgroundTaskRunner<IBackgroundTask>("LogScrubber", logger, _applicationShutdownRegistry);

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

                tasks.Add(RegisterScheduledPublishing());
                tasks.Add(RegisterLogScrubber(_loggingSettings));

                return tasks.ToArray();
            });
        }

        private IBackgroundTask RegisterScheduledPublishing()
        {
            // scheduled publishing/unpublishing
            // install on all, will only run on non-replica servers
            var task = new ScheduledPublishing(_publishingRunner, DefaultDelayMilliseconds, OneMinuteMilliseconds, _runtime, _mainDom, _serverRegistrar, _contentService, _umbracoContextFactory, _loggerFactory.CreateLogger<ScheduledPublishing>(), _serverMessenger, _backofficeSecurityFactory);
            _publishingRunner.TryAdd(task);
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
    }
}
