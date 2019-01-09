using Umbraco.Core.Cache;
using Umbraco.Core.Components;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class CompositionExtensions
    {
        #region Essentials

        /// <summary>
        /// Registers essential services.
        /// </summary>
        public static void RegisterEssentials(this Composition composition,
            ILogger logger, IProfiler profiler, IProfilingLogger profilingLogger,
            IMainDom mainDom,
            CacheHelper appCaches,
            IUmbracoDatabaseFactory databaseFactory,
            TypeLoader typeLoader,
            IRuntimeState state)
        {
            composition.RegisterUnique(logger);
            composition.RegisterUnique(profiler);
            composition.RegisterUnique(profilingLogger);
            composition.RegisterUnique(mainDom);
            composition.RegisterUnique(appCaches);
            composition.RegisterUnique(factory => factory.GetInstance<CacheHelper>().RuntimeCache);
            composition.RegisterUnique(databaseFactory);
            composition.RegisterUnique(factory => factory.GetInstance<IUmbracoDatabaseFactory>().SqlContext);
            composition.RegisterUnique(typeLoader);
            composition.RegisterUnique(state);
        }

        #endregion

        #region Unique

        /// <summary>
        /// Registers a unique service as its own implementation.
        /// </summary>
        public static void RegisterUnique<TService>(this Composition composition)
            => composition.RegisterUnique(typeof(TService), typeof(TService));

        /// <summary>
        /// Registers a unique service with an implementation type.
        /// </summary>
        public static void RegisterUnique<TService, TImplementing>(this Composition composition)
            => composition.RegisterUnique(typeof(TService), typeof(TImplementing));

        /// <summary>
        /// Registers a unique service with an implementation type, for a target.
        /// </summary>
        public static void RegisterUniqueFor<TService, TTarget, TImplementing>(this Composition composition)
            where TService : class
            => composition.RegisterUniqueFor<TService, TTarget>(typeof(TImplementing));

        /// <summary>
        /// Registers a unique service with an implementing instance.
        /// </summary>
        public static void RegisterUnique<TService>(this Composition composition, TService instance)
            => composition.RegisterUniqueInstance(typeof(TService), instance);

        #endregion
    }
}
