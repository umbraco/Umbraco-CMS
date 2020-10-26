using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Logging.Serilog;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Runtime;
using Umbraco.Infrastructure.Composing;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Profiler;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
using CoreDebugSettings = Umbraco.Core.Configuration.Models.CoreDebugSettings;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="appCaches"></param>
        /// <param name="loggingConfiguration"></param>
        /// <param name="factory"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IRegister umbContainer,
            Assembly entryAssembly,
            AppCaches appCaches,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
            => services.AddUmbracoCore(webHostEnvironment, umbContainer, entryAssembly, appCaches, loggingConfiguration, configuration, GetCoreRuntime);

        /// <summary>
        /// Adds the Umbraco Configuration requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            services.AddSingleton<IValidateOptions<ContentSettings>, ContentSettingsValidator>();
            services.AddSingleton<IValidateOptions<GlobalSettings>, GlobalSettingsValidator>();
            services.AddSingleton<IValidateOptions<RequestHandlerSettings>, RequestHandlerSettingsValidator>();

            services.Configure<ActiveDirectorySettings>(configuration.GetSection(Constants.Configuration.ConfigActiveDirectory));
            services.Configure<ConnectionStrings>(configuration.GetSection("ConnectionStrings"), o => o.BindNonPublicProperties = true);
            services.Configure<ContentSettings>(configuration.GetSection(Constants.Configuration.ConfigContent));
            services.Configure<CoreDebugSettings>(configuration.GetSection(Constants.Configuration.ConfigCoreDebug));
            services.Configure<ExceptionFilterSettings>(configuration.GetSection(Constants.Configuration.ConfigExceptionFilter));
            services.Configure<GlobalSettings>(configuration.GetSection(Constants.Configuration.ConfigGlobal));
            services.Configure<HealthChecksSettings>(configuration.GetSection(Constants.Configuration.ConfigHealthChecks));
            services.Configure<HostingSettings>(configuration.GetSection(Constants.Configuration.ConfigHosting));
            services.Configure<ImagingSettings>(configuration.GetSection(Constants.Configuration.ConfigImaging));
            services.Configure<IndexCreatorSettings>(configuration.GetSection(Constants.Configuration.ConfigExamine));
            services.Configure<KeepAliveSettings>(configuration.GetSection(Constants.Configuration.ConfigKeepAlive));
            services.Configure<LoggingSettings>(configuration.GetSection(Constants.Configuration.ConfigLogging));
            services.Configure<MemberPasswordConfigurationSettings>(configuration.GetSection(Constants.Configuration.ConfigMemberPassword));
            services.Configure<ModelsBuilderSettings>(configuration.GetSection(Constants.Configuration.ConfigModelsBuilder), o => o.BindNonPublicProperties = true);
            services.Configure<NuCacheSettings>(configuration.GetSection(Constants.Configuration.ConfigNuCache));
            services.Configure<RequestHandlerSettings>(configuration.GetSection(Constants.Configuration.ConfigRequestHandler));
            services.Configure<RuntimeSettings>(configuration.GetSection(Constants.Configuration.ConfigRuntime));
            services.Configure<SecuritySettings>(configuration.GetSection(Constants.Configuration.ConfigSecurity));
            services.Configure<TourSettings>(configuration.GetSection(Constants.Configuration.ConfigTours));
            services.Configure<TypeFinderSettings>(configuration.GetSection(Constants.Configuration.ConfigTypeFinder));
            services.Configure<UserPasswordConfigurationSettings>(configuration.GetSection(Constants.Configuration.ConfigUserPassword));
            services.Configure<WebRoutingSettings>(configuration.GetSection(Constants.Configuration.ConfigWebRouting));

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="configuration"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoCore(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {

            var loggingConfig = new LoggingConfiguration(
                Path.Combine(webHostEnvironment.ContentRootPath, "umbraco", "logs"));

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddSingleton<ILoggingConfiguration>(loggingConfig);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext?.Items);
            var appCaches = new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                requestCache,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

            /* TODO: MSDI - Post initial merge we can clean up a lot.
             * Change the method signatures lower down
             * Or even just remove IRegister / IFactory interfaces entirely.
             * If we try to do it immediately, merging becomes a nightmare.
             */
            var register = new ServiceCollectionRegistryAdapter(services);

            services.AddUmbracoCore(webHostEnvironment,
                register,
                Assembly.GetEntryAssembly(),
                appCaches,
                loggingConfig,
                configuration,
                GetCoreRuntime);

            return services;
        }

        /// <summary>
        /// Adds the Umbraco Back Core requirements
        /// </summary>
        /// <param name="services"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="umbContainer"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="requestCache"></param>
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
            AppCaches  appCaches,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration,
            //TODO: Yep that's extremely ugly
            Func<GlobalSettings, ConnectionStrings, IUmbracoVersion, IIOHelper, ILoggerFactory, IProfiler, IHostingEnvironment, IBackOfficeInfo, ITypeFinder, AppCaches, IDbProviderFactoryCreator, IRuntime> getRuntime)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            var container = umbContainer;
            if (container is null) throw new ArgumentNullException(nameof(container));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            // Add service session
            // This can be overwritten by the user by adding their own call to AddSession
            // since the last call of AddSession take precedence
            services.AddSession(options =>
            {
                options.Cookie.Name = "UMB_SESSION";
                options.Cookie.HttpOnly = true;
            });

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

            var globalSettings = serviceProvider.GetService<IOptionsMonitor<GlobalSettings>>();
            var connectionStrings = serviceProvider.GetService<IOptions<ConnectionStrings>>();
            var hostingSettings = serviceProvider.GetService<IOptionsMonitor<HostingSettings>>();
            var typeFinderSettings = serviceProvider.GetService<IOptionsMonitor<TypeFinderSettings>>();

            var dbProviderFactoryCreator = serviceProvider.GetRequiredService<IDbProviderFactoryCreator>();

            CreateCompositionRoot(services,
                globalSettings,
                hostingSettings,
                webHostEnvironment,
                loggingConfiguration,
                configuration, out var ioHelper, out var hostingEnvironment, out var backOfficeInfo, out var profiler);

            var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();

            var umbracoVersion = new UmbracoVersion();
            var typeFinder = CreateTypeFinder(loggerFactory, profiler, webHostEnvironment, entryAssembly, typeFinderSettings);

            var coreRuntime = getRuntime(
                globalSettings.CurrentValue,
                connectionStrings.Value,
                umbracoVersion,
                ioHelper,
                loggerFactory,
                profiler,
                hostingEnvironment,
                backOfficeInfo,
                typeFinder,
                appCaches,
                dbProviderFactoryCreator);

            coreRuntime.Configure(services);

            return services;
        }

        private static ITypeFinder CreateTypeFinder(ILoggerFactory loggerFactory, IProfiler profiler, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, IOptionsMonitor<TypeFinderSettings> typeFinderSettings)
        {
            var runtimeHashPaths = new RuntimeHashPaths();
            runtimeHashPaths.AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(loggerFactory.CreateLogger("RuntimeHash"), profiler), runtimeHashPaths);
            return new TypeFinder(loggerFactory.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(entryAssembly), runtimeHash, new TypeFinderConfig(typeFinderSettings));
        }

        private static IRuntime GetCoreRuntime(
           GlobalSettings globalSettings, ConnectionStrings connectionStrings, IUmbracoVersion umbracoVersion, IIOHelper ioHelper, ILoggerFactory loggerFactory,
            IProfiler profiler, IHostingEnvironment hostingEnvironment, IBackOfficeInfo backOfficeInfo,
            ITypeFinder typeFinder, AppCaches appCaches, IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = globalSettings.MainDomLock;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock" || isWindows == false
                ? (IMainDomLock)new SqlMainDomLock(loggerFactory.CreateLogger<SqlMainDomLock>(), loggerFactory, globalSettings, connectionStrings, dbProviderFactoryCreator, hostingEnvironment)
                : new MainDomSemaphoreLock(loggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);

            var mainDom = new MainDom(loggerFactory.CreateLogger<MainDom>(), mainDomLock);

            var coreRuntime = new CoreRuntime(
                globalSettings,
                connectionStrings,
                umbracoVersion,
                ioHelper,
                loggerFactory,
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
            IOptionsMonitor<GlobalSettings> globalSettings,
            IOptionsMonitor<HostingSettings> hostingSettings,
            IWebHostEnvironment webHostEnvironment,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration,
            out IIOHelper ioHelper,
            out Core.Hosting.IHostingEnvironment hostingEnvironment,
            out IBackOfficeInfo backOfficeInfo,
            out IProfiler profiler)
        {
            if (globalSettings == null)
                throw new InvalidOperationException($"Could not resolve type {typeof(GlobalSettings)} from the container, ensure {nameof(AddUmbracoConfiguration)} is called before calling {nameof(AddUmbracoCore)}");

            hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment);
            ioHelper = new IOHelper(hostingEnvironment);
            AddLogger(services, hostingEnvironment, loggingConfiguration, configuration);
            backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            profiler = GetWebProfiler(hostingEnvironment);

            return services;
        }

        /// <summary>
        /// Create and configure the logger
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        private static void AddLogger(
            IServiceCollection services,
            Core.Hosting.IHostingEnvironment hostingEnvironment,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
        {
            // Create a serilog logger
            var logger = SerilogLogger.CreateWithDefaultConfiguration(hostingEnvironment, loggingConfiguration, configuration);

            // This is nessasary to pick up all the loggins to MS ILogger.
            Log.Logger = logger.SerilogLog;


            // Wire up all the bits that serilog needs. We need to use our own code since the Serilog ext methods don't cater to our needs since
            // we don't want to use the global serilog `Log` object and we don't have our own ILogger implementation before the HostBuilder runs which
            // is the only other option that these ext methods allow.
            // I have created a PR to make this nicer https://github.com/serilog/serilog-extensions-hosting/pull/19 but we'll need to wait for that.
            // Also see : https://github.com/serilog/serilog-extensions-hosting/blob/dev/src/Serilog.Extensions.Hosting/SerilogHostBuilderExtensions.cs

            services.AddLogging(configure =>
            {
                configure.AddSerilog(logger.SerilogLog, false);
            });

            // This won't (and shouldn't) take ownership of the logger.
            services.AddSingleton(logger.SerilogLog);

            // Registered to provide two services...
            var diagnosticContext = new DiagnosticContext(logger.SerilogLog);

            // Consumed by e.g. middleware
            services.AddSingleton(diagnosticContext);

            // Consumed by user code
            services.AddSingleton<IDiagnosticContext>(diagnosticContext);
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
