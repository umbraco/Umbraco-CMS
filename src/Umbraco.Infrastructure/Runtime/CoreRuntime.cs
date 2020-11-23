using System;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Runtime
{
    public class CoreRuntime : IRuntime
    {
        public IRuntimeState State { get; }

        private readonly ILogger<CoreRuntime> _logger;
        private readonly ComponentCollection _components;
        private readonly IApplicationShutdownRegistry _applicationShutdownRegistry;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IMainDom _mainDom;
        private readonly IUmbracoDatabaseFactory _databaseFactory;

        public CoreRuntime(
            ILogger<CoreRuntime> logger,
            IRuntimeState state,
            ComponentCollection components,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IProfilingLogger profilingLogger,
            IMainDom mainDom,
            IUmbracoDatabaseFactory databaseFactory)
        {
            State = state;
            _logger = logger;
            _components = components;
            _applicationShutdownRegistry = applicationShutdownRegistry;
            _profilingLogger = profilingLogger;
            _mainDom = mainDom;
            _databaseFactory = databaseFactory;
        }
        

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                _logger.LogError(exception, msg);
            };

            DetermineRuntimeLevel();

            if (State.Level <= RuntimeLevel.BootFailed)
                throw new InvalidOperationException($"Cannot start the runtime if the runtime level is less than or equal to {RuntimeLevel.BootFailed}");

            var hostingEnvironmentLifetime = _applicationShutdownRegistry;
            if (hostingEnvironmentLifetime == null)
                throw new InvalidOperationException($"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(Start)}");

            // acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
            AcquireMainDom(_mainDom, _applicationShutdownRegistry);

            // create & initialize the components
            _components.Initialize();
        }

        public void Terminate()
        {
            _components?.Terminate();
        }

        private void AcquireMainDom(IMainDom mainDom, IApplicationShutdownRegistry applicationShutdownRegistry)
        {
            using (var timer = _profilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired."))
            {
                try
                {
                    mainDom.Acquire(applicationShutdownRegistry);
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
            using var timer = _profilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined.");

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

                // BOO a cast, yay no CoreRuntimeBootstrapper
                ((RuntimeState)State).Level = RuntimeLevel.BootFailed;
                ((RuntimeState)State).Reason = RuntimeLevelReason.BootFailedOnException;
                timer?.Fail();
                throw;
            }
        }
    }
}
