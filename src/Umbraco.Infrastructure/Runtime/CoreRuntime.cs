using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Infrastructure.Runtime
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

        public CoreRuntime(
            ILoggerFactory loggerFactory,
            IRuntimeState state,
            ComponentCollection components,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IProfilingLogger profilingLogger,
            IMainDom mainDom,
            IUmbracoDatabaseFactory databaseFactory,
            IEventAggregator eventAggregator,
            IHostingEnvironment hostingEnvironment)
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

            AppDomain.CurrentDomain.SetData("DataDirectory", _hostingEnvironment?.MapPathContentRoot(Core.Constants.SystemDirectories.Data));

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
