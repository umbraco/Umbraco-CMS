using System;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides a static service locator for most singletons.
    /// </summary>
    /// <remarks>
    /// <para>This class is initialized with the container in UmbracoApplicationBase,
    /// right after the container is created in UmbracoApplicationBase.HandleApplicationStart.</para>
    /// <para>Obviously, this is a service locator, which some may consider an anti-pattern. And yet,
    /// practically, it works.</para>
    /// </remarks>
    public static class CurrentCore
    {
        private static IFactory _factory;

        private static ILogger _logger;
        private static IProfiler _profiler;
        private static IProfilingLogger _profilingLogger;

        /// <summary>
        /// Gets or sets the factory.
        /// </summary>
        public static IFactory Factory
        {
            get
            {
                if (_factory == null) throw new InvalidOperationException("No factory has been set.");
                return _factory;
            }
            set
            {
                if (_factory != null) throw new InvalidOperationException("A factory has already been set.");
               // if (_configs != null) throw new InvalidOperationException("Configs are unlocked.");
                _factory = value;
            }
        }

        public static bool HasFactory => _factory != null;

        #region Getters

        public static ILogger Logger
            => _logger ?? (_logger = _factory?.TryGetInstance<ILogger>() ?? throw new Exception("TODO Fix")); //?? new DebugDiagnosticsLogger(new MessageTemplates()));

        public static IProfiler Profiler
            => _profiler ?? (_profiler = _factory?.TryGetInstance<IProfiler>()
                                         ?? new LogProfiler(Logger));

        public static IProfilingLogger ProfilingLogger
            => _profilingLogger ?? (_profilingLogger = _factory?.TryGetInstance<IProfilingLogger>())
               ?? new ProfilingLogger(Logger, Profiler);

        #endregion
    }
}
