using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using ComponentCollection = Umbraco.Cms.Core.Composing.ComponentCollection;

namespace Umbraco.Cms.Infrastructure.Runtime
{
    public class CoreRuntime : IRuntime
    {
        private readonly ILogger<CoreRuntime> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ComponentCollection _components;
        private readonly IApplicationShutdownRegistry _applicationShutdownRegistry;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IMainDom _mainDom;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IServiceProvider _serviceProvider;
        private CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRuntime"/> class.
        /// </summary>
        public CoreRuntime(
            ILoggerFactory loggerFactory,
            IRuntimeState state,
            ComponentCollection components,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IProfilingLogger profilingLogger,
            IMainDom mainDom,
            IUmbracoDatabaseFactory databaseFactory,
            IEventAggregator eventAggregator,
            IHostingEnvironment hostingEnvironment,
            IUmbracoVersion umbracoVersion,
            IServiceProvider serviceProvider)
        {
            State = state;
            _loggerFactory = loggerFactory;
            _components = components;
            _applicationShutdownRegistry = applicationShutdownRegistry;
            _profilingLogger = profilingLogger;
            _mainDom = mainDom;
            _databaseFactory = databaseFactory;
            _eventAggregator = eventAggregator;
            _hostingEnvironment = hostingEnvironment;
            _umbracoVersion = umbracoVersion;
            _serviceProvider = serviceProvider;
            _logger = _loggerFactory.CreateLogger<CoreRuntime>();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public CoreRuntime(
            ILoggerFactory loggerFactory,
            IRuntimeState state,
            ComponentCollection components,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IProfilingLogger profilingLogger,
            IMainDom mainDom,
            IUmbracoDatabaseFactory databaseFactory,
            IEventAggregator eventAggregator,
            IHostingEnvironment hostingEnvironment,
            IUmbracoVersion umbracoVersion
            ):this(
            loggerFactory,
            state,
            components,
            applicationShutdownRegistry,
            profilingLogger,
            mainDom,
            databaseFactory,
            eventAggregator,
            hostingEnvironment,
            umbracoVersion,
            null
            )
        {

        }

        /// <summary>
        /// Gets the state of the Umbraco runtime.
        /// </summary>
        public IRuntimeState State { get; }

        /// <inheritdoc/>
        public async Task RestartAsync()
        {
            await StopAsync(_cancellationToken);
            await StartAsync(_cancellationToken);
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            StaticApplicationLogging.Initialize(_loggerFactory);
            StaticServiceProvider.Instance = _serviceProvider;

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";

                if (isTerminating)
                {
                    msg += " (terminating)";
                }

                msg += ".";

                _logger.LogError(exception, msg);
            };

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom();

            await _eventAggregator.PublishAsync(new UmbracoApplicationMainDomAcquiredNotification(), cancellationToken);

            // notify for unattended install
            await _eventAggregator.PublishAsync(new RuntimeUnattendedInstallNotification());
            DetermineRuntimeLevel();

            if (!State.UmbracoCanBoot())
            {
                return; // The exception will be rethrown by BootFailedMiddelware
            }

            IApplicationShutdownRegistry hostingEnvironmentLifetime = _applicationShutdownRegistry;
            if (hostingEnvironmentLifetime == null)
            {
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(StartAsync)}");
            }

            // if level is Run and reason is UpgradeMigrations, that means we need to perform an unattended upgrade
            var unattendedUpgradeNotification = new RuntimeUnattendedUpgradeNotification();
            await _eventAggregator.PublishAsync(unattendedUpgradeNotification);
            switch (unattendedUpgradeNotification.UnattendedUpgradeResult)
            {
                case RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors:
                    if (State.BootFailedException == null)
                    {
                        throw new InvalidOperationException($"Unattended upgrade result was {RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors} but no {nameof(BootFailedException)} was registered");
                    }
                    // we cannot continue here, the exception will be rethrown by BootFailedMiddelware
                    return;
                case RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete:
                case RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete:
                    // upgrade is done, set reason to Run
                    DetermineRuntimeLevel();
                    break;
                case RuntimeUnattendedUpgradeNotification.UpgradeResult.NotRequired:
                    break;
            }

            await _eventAggregator.PublishAsync(new UmbracoApplicationComponentsInstallingNotification(State.Level), cancellationToken);

            // create & initialize the components
            _components.Initialize();

            await _eventAggregator.PublishAsync(new UmbracoApplicationStartingNotification(State.Level), cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _components.Terminate();
            await _eventAggregator.PublishAsync(new UmbracoApplicationStoppingNotification(), cancellationToken);
            StaticApplicationLogging.Initialize(null);
        }

        private void AcquireMainDom()
        {
            using (DisposableTimer timer = _profilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
                    _mainDom.Acquire(_applicationShutdownRegistry);
                }
                catch
                {
                    timer?.Fail();
                    throw;
                }
            }
        }

        private void DetermineRuntimeLevel()
        {
            if (State.BootFailedException != null)
            {
                // there's already been an exception so cannot boot and no need to check
                return;
            }

            using DisposableTimer timer = _profilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined.");

            try
            {
                State.DetermineRuntimeLevel();

                _logger.LogDebug("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", State.Level, State.Reason);

                if (State.Level == RuntimeLevel.Upgrade)
                {
                    _logger.LogDebug("Configure database factory for upgrades.");
                    _databaseFactory.ConfigureForUpgrade();
                }
            }
            catch (Exception ex)
            {
                State.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException);
                timer?.Fail();
                _logger.LogError(ex, "Boot Failed");
                // We do not throw the exception. It will be rethrown by BootFailedMiddleware
            }
        }
    }
}
