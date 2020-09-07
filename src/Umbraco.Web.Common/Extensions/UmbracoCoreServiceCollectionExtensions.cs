using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using Umbraco.Configuration;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Runtime;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Common.Profiler;

namespace Umbraco.Extensions
{


    public static class UmbracoCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SqlCe support for Umbraco
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoSqlCeSupport(this IServiceCollection services)
        {
            try
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (binFolder != null)
                {
                    var dllPath = Path.Combine(binFolder, "Umbraco.Persistance.SqlCe.dll");
                    var umbSqlCeAssembly = Assembly.LoadFrom(dllPath);

                    var sqlCeSyntaxProviderType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeSyntaxProvider");
                    var sqlCeBulkSqlInsertProviderType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeBulkSqlInsertProvider");
                    var sqlCeEmbeddedDatabaseCreatorType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeEmbeddedDatabaseCreator");

                    if (!(sqlCeSyntaxProviderType is null || sqlCeBulkSqlInsertProviderType is null || sqlCeEmbeddedDatabaseCreatorType is null))
                    {
                        services.AddSingleton(typeof(ISqlSyntaxProvider), sqlCeSyntaxProviderType);
                        services.AddSingleton(typeof(IBulkSqlInsertProvider), sqlCeBulkSqlInsertProviderType);
                        services.AddSingleton(typeof(IEmbeddedDatabaseCreator), sqlCeEmbeddedDatabaseCreatorType);
                    }

                    var sqlCeAssembly = Assembly.LoadFrom(Path.Combine(binFolder, "System.Data.SqlServerCe.dll"));

                    var sqlCe = sqlCeAssembly.GetType("System.Data.SqlServerCe.SqlCeProviderFactory");
                    if (!(sqlCe is null))
                    {
                        DbProviderFactories.RegisterFactory(Core.Constants.DbProviderNames.SqlCe, sqlCe);
                    }
                }
            }
            catch
            {
                // Ignore if SqlCE is not available
            }

            return services;
        }

        /// <summary>
        /// Adds Sql Server support for Umbraco
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoSqlServerSupport(this IServiceCollection services)
        {
            DbProviderFactories.RegisterFactory(Core.Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            services.AddSingleton<ISqlSyntaxProvider, SqlServerSyntaxProvider>();
            services.AddSingleton<IBulkSqlInsertProvider, SqlServerBulkSqlInsertProvider>();
            services.AddSingleton<IEmbeddedDatabaseCreator, NoopEmbeddedDatabaseCreator>();

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Configuration requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var configsFactory = new AspNetCoreConfigsFactory(configuration);

            var configs = configsFactory.Create();

            services.AddSingleton(configs);

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment)
        {
            return services.AddUmbracoCore(webHostEnvironment, out _);
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, out IFactory factory)
        {
            if (!UmbracoServiceProviderFactory.IsActive)
                throw new InvalidOperationException("Ensure to add UseUmbraco() in your Program.cs after ConfigureWebHostDefaults to enable Umbraco's service provider factory");

            var umbContainer = UmbracoServiceProviderFactory.UmbracoContainer;

            var loggingConfig = new LoggingConfiguration(
                Path.Combine(webHostEnvironment.ContentRootPath, "App_Data", "Logs"),
                Path.Combine(webHostEnvironment.ContentRootPath, "config", "serilog.config"),
                Path.Combine(webHostEnvironment.ContentRootPath, "config", "serilog.user.config"));

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext?.Items);
            var appCaches = new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                requestCache,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

            services.AddUmbracoCore(webHostEnvironment,
                umbContainer,
                Assembly.GetEntryAssembly(),
                appCaches,
                loggingConfig,
                out factory);

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="appCaches"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggingConfiguration"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IRegister umbContainer,
            Assembly entryAssembly,
            AppCaches appCaches,
            ILoggingConfiguration loggingConfiguration,
            out IFactory factory)
            => services.AddUmbracoCore(webHostEnvironment, umbContainer, entryAssembly, appCaches, loggingConfiguration, GetCoreRuntime, out factory);


        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="appCaches"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggingConfiguration"></param>
        /// <param name="getRuntime">Delegate to create an <see cref="IRuntime"/></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IRegister umbContainer,
            Assembly entryAssembly,
            AppCaches appCaches,
            ILoggingConfiguration loggingConfiguration,
            // TODO: Yep that's extremely ugly
            Func<Configs, IUmbracoVersion, IIOHelper, Core.Logging.ILogger, IProfiler, Core.Hosting.IHostingEnvironment, IBackOfficeInfo, ITypeFinder, AppCaches, IDbProviderFactoryCreator, IRuntime> getRuntime,
            out IFactory factory)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            var container = umbContainer;
            if (container is null) throw new ArgumentNullException(nameof(container));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            // Add supported databases
            services.AddUmbracoSqlCeSupport();
            services.AddUmbracoSqlServerSupport();

            services.AddSingleton<IDbProviderFactoryCreator>(x => new DbProviderFactoryCreator(
                DbProviderFactories.GetFactory,
                x.GetServices<ISqlSyntaxProvider>(),
                x.GetServices<IBulkSqlInsertProvider>(),
                x.GetServices<IEmbeddedDatabaseCreator>()
            ));

            // TODO: We want to avoid pre-resolving a container as much as possible we should not
            // be doing this any more than we are now. The ugly part about this is that the service
            // instances resolved here won't be the same instances resolved from the container
            // later once the true container is built. However! ... in the case of IDbProviderFactoryCreator
            // it will be the same instance resolved later because we are re-registering this instance back
            // into the container. This is not true for `Configs` but we should do that too, see comments in
            // `RegisterEssentials`.
            var serviceProvider = services.BuildServiceProvider();
            var configs = serviceProvider.GetService<Configs>();
            var dbProviderFactoryCreator = serviceProvider.GetRequiredService<IDbProviderFactoryCreator>();

            CreateCompositionRoot(services,
                configs,
                webHostEnvironment,
                loggingConfiguration,
                out var logger, out var ioHelper, out var hostingEnvironment, out var backOfficeInfo, out var profiler);

            var globalSettings = configs.Global();
            var umbracoVersion = new UmbracoVersion(globalSettings);
            var typeFinder = CreateTypeFinder(logger, profiler, webHostEnvironment, entryAssembly, configs.TypeFinder());

            var runtime = getRuntime(
                configs,                
                umbracoVersion,
                ioHelper,
                logger,
                profiler,
                hostingEnvironment,
                backOfficeInfo,
                typeFinder,
                appCaches,
                dbProviderFactoryCreator);

            factory = runtime.Configure(container);

            return services;
        }

        private static ITypeFinder CreateTypeFinder(Core.Logging.ILogger logger, IProfiler profiler, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, ITypeFinderSettings typeFinderSettings)
        {
            var runtimeHashPaths = new RuntimeHashPaths();
            runtimeHashPaths.AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(logger, profiler), runtimeHashPaths);
            return new TypeFinder(logger, new DefaultUmbracoAssemblyProvider(entryAssembly), runtimeHash, new TypeFinderConfig(typeFinderSettings));
        }

        private static IRuntime GetCoreRuntime(
            Configs configs, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, Core.Logging.ILogger logger,
            IProfiler profiler, Core.Hosting.IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder, AppCaches appCaches, IDbProviderFactoryCreator dbProviderFactoryCreator)
        {

            // Determine if we should use the sql main dom or the default
            var globalSettings = configs.Global();
            var connStrings = configs.ConnectionStrings();
            var appSettingMainDomLock = globalSettings.MainDomLock;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock" || isWindows == false
                ? (IMainDomLock)new SqlMainDomLock(logger, globalSettings, connStrings, dbProviderFactoryCreator, hostingEnvironment)
                : new MainDomSemaphoreLock(logger, hostingEnvironment);

            var mainDom = new MainDom(logger, mainDomLock);

            var coreRuntime = new CoreRuntime(
                configs,                
                umbracoVersion,
                ioHelper,
                logger,
                profiler,
                new AspNetCoreBootPermissionsChecker(),
                hostingEnvironment,
                backOfficeInfo,
                dbProviderFactoryCreator,
                mainDom,
                typeFinder,
                appCaches);

            return coreRuntime;
        }

        private static IServiceCollection CreateCompositionRoot(
            IServiceCollection services,
            Configs configs,
            IWebHostEnvironment webHostEnvironment,
            ILoggingConfiguration loggingConfiguration,
            out Core.Logging.ILogger logger,
            out IIOHelper ioHelper,
            out Core.Hosting.IHostingEnvironment hostingEnvironment,
            out IBackOfficeInfo backOfficeInfo,
            out IProfiler profiler)
        {
            if (configs == null)
                throw new InvalidOperationException($"Could not resolve type {typeof(Configs)} from the container, ensure {nameof(AddUmbracoConfiguration)} is called before calling {nameof(AddUmbracoCore)}");

            var hostingSettings = configs.Hosting();
            var globalSettings = configs.Global();

            hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment);
            ioHelper = new IOHelper(hostingEnvironment);
            logger = AddLogger(services, hostingEnvironment, loggingConfiguration);

            backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            profiler = GetWebProfiler(hostingEnvironment);

            return services;
        }

        /// <summary>
        /// Create and configure the logger
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        private static Core.Logging.ILogger AddLogger(IServiceCollection services, Core.Hosting.IHostingEnvironment hostingEnvironment, ILoggingConfiguration loggingConfiguration)
        {
            // Create a serilog logger
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration);

            // Wire up all the bits that serilog needs. We need to use our own code since the Serilog ext methods don't cater to our needs since
            // we don't want to use the global serilog `Log` object and we don't have our own ILogger implementation before the HostBuilder runs which
            // is the only other option that these ext methods allow.
            // I have created a PR to make this nicer https://github.com/serilog/serilog-extensions-hosting/pull/19 but we'll need to wait for that.
            // Also see : https://github.com/serilog/serilog-extensions-hosting/blob/dev/src/Serilog.Extensions.Hosting/SerilogHostBuilderExtensions.cs

            services.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory(logger.SerilogLog, false));

            // This won't (and shouldn't) take ownership of the logger.
            services.AddSingleton(logger.SerilogLog);

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger.SerilogLog);

            // Consumed by e.g. middleware
            services.AddSingleton(diagnosticContext);

            // Consumed by user code
            services.AddSingleton<IDiagnosticContext>(diagnosticContext);

            return logger;
        }

        private static IProfiler GetWebProfiler(Umbraco.Core.Hosting.IHostingEnvironment hostingEnvironment)
        {
            // create and start asap to profile boot
            if (!hostingEnvironment.IsDebugMode)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            var webProfiler = new WebProfiler();
            webProfiler.StartBoot();

            return webProfiler;
        }

        private class AspNetCoreBootPermissionsChecker : IUmbracoBootPermissionChecker
        {
            public void ThrowIfNotPermissions()
            {
                // nothing to check
            }
        }

    }

}
