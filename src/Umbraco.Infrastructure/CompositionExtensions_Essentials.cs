using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
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
        /// <remarks>
        /// These services are all either created by the runtime or used to construct the runtime
        /// </remarks>
        public static void RegisterEssentials(this Composition composition,
            // TODO: Configs should be here too, the reason is that we only register them before the Core Runtime in aspnetcore
            // then we pre-resolve them which means that the instance re-resolved later is different... BUT if we register that
            // pre-resolved instance here again, then it will be the same instance re-resolved later, just like we are doing with
            // IDbProviderFactoryCreator.
            ILogger logger, ILoggerFactory loggerFactory, IProfiler profiler, IProfilingLogger profilingLogger,
            IMainDom mainDom,
            AppCaches appCaches,
            IUmbracoDatabaseFactory databaseFactory,
            TypeLoader typeLoader,
            IRuntimeState state,
            ITypeFinder typeFinder,
            IIOHelper ioHelper,
            IUmbracoVersion umbracoVersion,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo)
        {
            composition.RegisterUnique(logger);
            composition.RegisterUnique(loggerFactory);
            composition.RegisterUnique(profiler);
            composition.RegisterUnique(profilingLogger);
            composition.RegisterUnique(mainDom);
            composition.RegisterUnique(appCaches);
            composition.RegisterUnique(appCaches.RequestCache);
            composition.RegisterUnique(databaseFactory);
            composition.RegisterUnique(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
            composition.RegisterUnique(typeLoader);
            composition.RegisterUnique(state);
            composition.RegisterUnique(typeFinder);
            composition.RegisterUnique(ioHelper);
            composition.RegisterUnique(umbracoVersion);
            composition.RegisterUnique(dbProviderFactoryCreator);
            composition.RegisterUnique(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);
            composition.RegisterUnique(hostingEnvironment);
            composition.RegisterUnique(backOfficeInfo);
        }
    }
}
