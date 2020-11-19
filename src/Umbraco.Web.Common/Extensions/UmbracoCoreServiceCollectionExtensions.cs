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
using Umbraco.Core;
using Umbraco.Core.Builder;
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
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Profiler;
using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Extensions
{
    public static class UmbracoCoreServiceCollectionExtensions
    {
    

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
        /// <param name="getRuntimeBootstrapper">Delegate to create an <see cref="CoreRuntimeBootstrapper"/></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static IUmbracoBuilder AddUmbracoCore(
            this IUmbracoBuilder builder,
            IWebHostEnvironment webHostEnvironment,
            Assembly entryAssembly,
            AppCaches  appCaches,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration,
            //TODO: Yep that's extremely ugly
            Func<GlobalSettings, ConnectionStrings, IUmbracoVersion, IIOHelper, ILoggerFactory, IProfiler, IHostingEnvironment, IBackOfficeInfo, ITypeFinder, AppCaches, IDbProviderFactoryCreator, CoreRuntimeBootstrapper> getRuntimeBootstrapper)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));
            if (entryAssembly is null) throw new ArgumentNullException(nameof(entryAssembly));

            builder.Services.AddLazySupport();

            // Add service session
            // This can be overwritten by the user by adding their own call to AddSession
            // since the last call of AddSession take precedence
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "UMB_SESSION";
                options.Cookie.HttpOnly = true;
            });

            // Add supported databases
            builder.Services.AddUmbracoSqlCeSupport();
            builder.Services.AddUmbracoSqlServerSupport();

            builder.Services.AddSingleton<IDbProviderFactoryCreator>(x => new DbProviderFactoryCreator(
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
            var serviceProvider = builder.Services.BuildServiceProvider();

            var globalSettings = serviceProvider.GetService<IOptionsMonitor<GlobalSettings>>();
            var connectionStrings = serviceProvider.GetService<IOptions<ConnectionStrings>>();
            var hostingSettings = serviceProvider.GetService<IOptionsMonitor<HostingSettings>>();
            var typeFinderSettings = serviceProvider.GetService<IOptionsMonitor<TypeFinderSettings>>();

            var dbProviderFactoryCreator = serviceProvider.GetRequiredService<IDbProviderFactoryCreator>();

            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment);
            var ioHelper = new IOHelper(hostingEnvironment);
            var backOfficeInfo = new AspNetCoreBackOfficeInfo(globalSettings);
            var profiler = GetWebProfiler(hostingEnvironment);

            builder.Services.AddLogger(hostingEnvironment, loggingConfiguration, configuration);
            var loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

            var umbracoVersion = new UmbracoVersion();
            var typeFinder = CreateTypeFinder(loggerFactory, profiler, webHostEnvironment, entryAssembly, typeFinderSettings);

            var bootstrapper = getRuntimeBootstrapper(
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

            bootstrapper.Configure(builder);

            return builder;
        }

        /// <summary>
        /// Adds SqlCe support for Umbraco
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddUmbracoSqlCeSupport(this IServiceCollection services)
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
        /// Adds hosted services for Umbraco.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<HealthCheckNotifier>();
            services.AddHostedService<KeepAlive>();
            services.AddHostedService<LogScrubber>();
            services.AddHostedService<ScheduledPublishing>();
            services.AddHostedService<TempFileCleanup>();

            services.AddHostedService<InstructionProcessTask>();
            services.AddHostedService<TouchServerTask>();

            return services;
        }

        /// <summary>
        /// Adds HTTP clients for Umbraco.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddUmbracoHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient();
            return services;
        }

        private static ITypeFinder CreateTypeFinder(ILoggerFactory loggerFactory, IProfiler profiler, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, IOptionsMonitor<TypeFinderSettings> typeFinderSettings)
        {
            var runtimeHashPaths = new RuntimeHashPaths();
            runtimeHashPaths.AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(loggerFactory.CreateLogger("RuntimeHash"), profiler), runtimeHashPaths);
            return new TypeFinder(loggerFactory.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(entryAssembly), runtimeHash, new TypeFinderConfig(typeFinderSettings));
        }

        internal static CoreRuntimeBootstrapper GetCoreRuntime(
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

            var coreRuntime = new CoreRuntimeBootstrapper(
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


        /// <summary>
        /// Create and configure the logger
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public static IServiceCollection AddLogger(
            this IServiceCollection services,
            IHostingEnvironment hostingEnvironment,
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

            return services;
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
