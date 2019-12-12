using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
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
            IRuntimeState state,
            ITypeFinder typeFinder,
            IIOHelper ioHelper,
            IUmbracoVersion umbracoVersion,
            IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            composition.RegisterUnique(logger);
            composition.RegisterUnique(profiler);
            composition.RegisterUnique(profilingLogger);
            composition.RegisterUnique(mainDom);
            composition.RegisterUnique(appCaches);
            composition.RegisterUnique(appCaches.RequestCache);
            composition.RegisterUnique(databaseFactory);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoDatabaseFactory>().SqlContext);
            composition.RegisterUnique(typeLoader);
            composition.RegisterUnique(state);
            composition.RegisterUnique(typeFinder);
            composition.RegisterUnique(ioHelper);
            composition.RegisterUnique(umbracoVersion);
            composition.RegisterUnique(dbProviderFactoryCreator);
        }
    }
}
