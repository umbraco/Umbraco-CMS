using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;

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
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IUmbracoVersion _umbracoVersion;

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
            DatabaseBuilder databaseBuilder,
            IUmbracoVersion umbracoVersion)
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
            _databaseBuilder = databaseBuilder;
            _umbracoVersion = umbracoVersion;
            _logger = _loggerFactory.CreateLogger<CoreRuntime>();
        }

        /// <summary>
        /// Gets the state of the Umbraco runtime.
        /// </summary>
        public IRuntimeState State { get; }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            StaticApplicationLogging.Initialize(_loggerFactory);

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

            AppDomain.CurrentDomain.SetData("DataDirectory", _hostingEnvironment?.MapPathContentRoot(Constants.SystemDirectories.Data));

            DoUnattendedInstall();
            DetermineRuntimeLevel();

            if (State.Level <= RuntimeLevel.BootFailed)
            {
                return; // The exception will be rethrown by BootFailedMiddelware
            }

            IApplicationShutdownRegistry hostingEnvironmentLifetime = _applicationShutdownRegistry;
            if (hostingEnvironmentLifetime == null)
            {
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(StartAsync)}");
            }

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom();

            // if level is Run and reason is UpgradeMigrations, that means we need to perform an unattended upgrade
            if (State.Reason == RuntimeLevelReason.UpgradeMigrations && State.Level == RuntimeLevel.Run)
            {
                // do the upgrade
                DoUnattendedUpgrade();

                // upgrade is done, set reason to Run
                DetermineRuntimeLevel();

            }

            // create & initialize the components
            _components.Initialize();

            await _eventAggregator.PublishAsync(new UmbracoApplicationStartingNotification(State.Level), cancellationToken);
        }

        private void DoUnattendedUpgrade()
        {
            var plan = new UmbracoPlan(_umbracoVersion);
            using (_profilingLogger.TraceDuration<RuntimeState>("Starting unattended upgrade.", "Unattended upgrade completed."))
            {
                var result = _databaseBuilder.UpgradeSchemaAndData(plan);
                if (result.Success == false)
                    throw new UnattendedInstallException("An error occurred while running the unattended upgrade.\n" + result.Message);
            }

        }

        private void DoUnattendedInstall()
        {
            State.DoUnattendedInstall();
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
