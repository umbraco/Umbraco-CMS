using System;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Runtime
{
    public class CoreRuntime : IRuntime
    {
        public IRuntimeState State { get; }

        private readonly ComponentCollection _components;
        private readonly IUmbracoBootPermissionChecker _umbracoBootPermissionChecker;
        private readonly IApplicationShutdownRegistry _applicationShutdownRegistry;
        private readonly IProfilingLogger _profilingLogger;
        private readonly IMainDom _mainDom;

        public CoreRuntime(
            IRuntimeState state,
            ComponentCollection components,
            IUmbracoBootPermissionChecker umbracoBootPermissionChecker,
            IApplicationShutdownRegistry applicationShutdownRegistry,
            IProfilingLogger profilingLogger,
            IMainDom mainDom)
        {
            State = state;
            _components = components;
            _umbracoBootPermissionChecker = umbracoBootPermissionChecker;
            _applicationShutdownRegistry = applicationShutdownRegistry;
            _profilingLogger = profilingLogger;
            _mainDom = mainDom;
        }
        

        public void Start()
        {
            if (State.Level <= RuntimeLevel.BootFailed)
                throw new InvalidOperationException($"Cannot start the runtime if the runtime level is less than or equal to {RuntimeLevel.BootFailed}");

            // throws if not full-trust
            _umbracoBootPermissionChecker.ThrowIfNotPermissions();

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
    }
}
