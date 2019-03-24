using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static partial class CompositionExtensions
    {
        /// <summary>
        /// Registers essential services.
        /// </summary>
        public static void RegisterEssentials(this Composition composition,
            ILogger logger, IProfiler profiler, IProfilingLogger profilingLogger,
            IMainDom mainDom,
            AppCaches appCaches,
            IUmbracoDatabaseFactory databaseFactory,
            TypeLoader typeLoader,
            IRuntimeState state)
        {
            composition.RegisterUnique(logger);
            composition.RegisterUnique(profiler);
            composition.RegisterUnique(profilingLogger);
            composition.RegisterUnique(mainDom);
            composition.RegisterUnique(appCaches);
            composition.RegisterUnique(databaseFactory);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoDatabaseFactory>().SqlContext);
            composition.RegisterUnique(typeLoader);
            composition.RegisterUnique(state);
        }
    }
}
