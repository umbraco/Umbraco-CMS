using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;

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
            IServiceScopeFactory  serviceScopeFactory)
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
            _serviceScopeFactory = serviceScopeFactory;
            _logger = _loggerFactory.CreateLogger<CoreRuntime>();
        }

        /// <summary>
        /// Gets the state of the Umbraco runtime.
        /// </summary>
        public IRuntimeState State { get; }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();

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
                throw new InvalidOperationException($"Cannot start the runtime if the runtime level is less than or equal to {RuntimeLevel.BootFailed}");
            }

            IApplicationShutdownRegistry hostingEnvironmentLifetime = _applicationShutdownRegistry;
            if (hostingEnvironmentLifetime == null)
            {
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(StartAsync)}");
            }

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom();

            await _eventAggregator.PublishAsync(new UmbracoApplicationStarting(State.Level), cancellationToken);

            // create & initialize the components
            _components.Initialize();
        }

        private void DoUnattendedInstall()
        {
            State.DoUnattendedInstall();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _components.Terminate();
            await _eventAggregator.PublishAsync(new UmbracoApplicationStopping(), cancellationToken);
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
            catch
            {
                State.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException);
                timer?.Fail();
                throw;
            }
        }
    }
}
