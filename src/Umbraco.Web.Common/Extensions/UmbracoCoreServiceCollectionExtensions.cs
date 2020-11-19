using System;
using System.Collections.Generic;
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
        /// <param name="builder"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="entryAssembly"></param>
        /// <param name="appCaches"></param>
        /// <param name="loggingConfiguration"></param>
        /// <param name="configuration"></param>
        /// <param name="configureSomeMoreBits">Weird hack for tests, won't exist much longer</param>
        /// <returns></returns>
        /// <remarks>Shouldn't exist</remarks>
        public static IUmbracoBuilder AddUmbracoCore(
            this IUmbracoBuilder builder,
            IWebHostEnvironment webHostEnvironment,
            Assembly entryAssembly,
            AppCaches appCaches,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration,
            //TODO: Yep that's extremely ugly
            Action<IUmbracoBuilder, GlobalSettings, ConnectionStrings, IUmbracoVersion, IIOHelper,  IProfiler, IHostingEnvironment, ITypeFinder, AppCaches, IDbProviderFactoryCreator> configureSomeMoreBits)
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
            var syntaxProviders = new List<ISqlSyntaxProvider>();
            var insertProviders = new List<IBulkSqlInsertProvider>();
            var databaseCreators = new List<IEmbeddedDatabaseCreator>();

            // Add supported databases
            builder.Services.AddUmbracoSqlCeSupport(syntaxProviders, insertProviders, databaseCreators);
            builder.Services.AddUmbracoSqlServerSupport(syntaxProviders, insertProviders, databaseCreators);

            var dbProviderFactoryCreator = new DbProviderFactoryCreator(
                DbProviderFactories.GetFactory,
                syntaxProviders,
                insertProviders,
                databaseCreators);

            builder.Services.AddSingleton<IDbProviderFactoryCreator>(dbProviderFactoryCreator);

            // TODO: We should not be doing this at all.
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Switching to IOptions vs IOptionsMonitor was rejected previously as it prevents setting IsDebug true without a restart
            var hostingSettings = serviceProvider.GetService<IOptionsMonitor<HostingSettings>>();  // <--- We are now building ServiceProvider just for this line
            var hostingEnvironment = new AspNetCoreHostingEnvironment(hostingSettings, webHostEnvironment);
            var ioHelper = new IOHelper(hostingEnvironment);
            var profiler = GetWebProfiler(hostingEnvironment);

            builder.Services.AddUnique<IBackOfficeInfo, AspNetCoreBackOfficeInfo>();

            builder.Services.AddLogger(loggingConfiguration, configuration);
            var loggerFactory = builder.BuilderLoggerFactory;

            var typeFinderSettings = builder.Config.GetSection(Core.Constants.Configuration.ConfigTypeFinder).Get<TypeFinderSettings>() ?? new TypeFinderSettings();
            var typeFinder = CreateTypeFinder(loggerFactory, profiler, webHostEnvironment, entryAssembly, Options.Create(typeFinderSettings));
            var globalSettings = builder.Config.GetSection(Core.Constants.Configuration.ConfigGlobal).Get<GlobalSettings>() ?? new GlobalSettings();
            var connectionStrings = builder.Config.GetSection("ConnectionStrings").Get<ConnectionStrings>(opt => opt.BindNonPublicProperties = true) ?? new ConnectionStrings();

            configureSomeMoreBits(
                builder,
                globalSettings,
                connectionStrings,
                new UmbracoVersion(),
                ioHelper,
                profiler,
                hostingEnvironment,
                typeFinder,
                appCaches,
                dbProviderFactoryCreator);

            builder.AddComposers();

            return builder;
        }

        public static IUmbracoBuilder AddComposers(this IUmbracoBuilder builder)
        {
            var composerTypes = builder.TypeLoader.GetTypes<IComposer>();
            var enableDisable = builder.TypeLoader.GetAssemblyAttributes(typeof(EnableComposerAttribute), typeof(DisableComposerAttribute));
            new Composers(builder, composerTypes, enableDisable, builder.BuilderLoggerFactory.CreateLogger<Composers>()).Compose();

            return builder;
        }

        /// <summary>
        /// Adds SqlCe support for Umbraco
        /// </summary>
        private static IServiceCollection AddUmbracoSqlCeSupport(
            this IServiceCollection services,
            ICollection<ISqlSyntaxProvider> syntaxProviders,
            ICollection<IBulkSqlInsertProvider> insertProviders,
            ICollection<IEmbeddedDatabaseCreator> databaseCreators)
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

                        syntaxProviders.Add((ISqlSyntaxProvider)Activator.CreateInstance(sqlCeSyntaxProviderType));
                        insertProviders.Add((IBulkSqlInsertProvider)Activator.CreateInstance(sqlCeBulkSqlInsertProviderType));
                        databaseCreators.Add((IEmbeddedDatabaseCreator)Activator.CreateInstance(sqlCeEmbeddedDatabaseCreatorType));
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
        public static IServiceCollection AddUmbracoSqlServerSupport(
            this IServiceCollection services,
            ICollection<ISqlSyntaxProvider> syntaxProviders,
            ICollection<IBulkSqlInsertProvider> insertProviders,
            ICollection<IEmbeddedDatabaseCreator> databaseCreators)
        {
            DbProviderFactories.RegisterFactory(Core.Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            var syntaxProvider = new SqlServerSyntaxProvider();
            var insertProvider = new SqlServerBulkSqlInsertProvider();
            var databaseCreator = new NoopEmbeddedDatabaseCreator();

            services.AddSingleton<ISqlSyntaxProvider>(syntaxProvider);
            services.AddSingleton<IBulkSqlInsertProvider>(insertProvider);
            services.AddSingleton<IEmbeddedDatabaseCreator>(databaseCreator);

            syntaxProviders.Add(syntaxProvider);
            insertProviders.Add(insertProvider);
            databaseCreators.Add(databaseCreator);

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

        private static ITypeFinder CreateTypeFinder(ILoggerFactory loggerFactory, IProfiler profiler, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, IOptions<TypeFinderSettings> typeFinderSettings)
        {
            var runtimeHashPaths = new RuntimeHashPaths();
            runtimeHashPaths.AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(loggerFactory.CreateLogger("RuntimeHash"), profiler), runtimeHashPaths);
            return new TypeFinder(loggerFactory.CreateLogger<TypeFinder>(), new DefaultUmbracoAssemblyProvider(entryAssembly), runtimeHash, new TypeFinderConfig(typeFinderSettings));
        }

        internal static void ConfigureSomeMorebits(
            IUmbracoBuilder builder,
            GlobalSettings globalSettings,
            ConnectionStrings connectionStrings,
            IUmbracoVersion umbracoVersion,
            IIOHelper ioHelper,
            IProfiler profiler,
            IHostingEnvironment hostingEnvironment,
            ITypeFinder typeFinder,
            AppCaches appCaches,
            IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            // Determine if we should use the sql main dom or the default
            var appSettingMainDomLock = globalSettings.MainDomLock;

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var mainDomLock = appSettingMainDomLock == "SqlMainDomLock" || isWindows == false
                ? (IMainDomLock)new SqlMainDomLock(builder.BuilderLoggerFactory.CreateLogger<SqlMainDomLock>(), builder.BuilderLoggerFactory, globalSettings, connectionStrings, dbProviderFactoryCreator, hostingEnvironment)
                : new MainDomSemaphoreLock(builder.BuilderLoggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);

            var mainDom = new MainDom(builder.BuilderLoggerFactory.CreateLogger<MainDom>(), mainDomLock);


            var logger = builder.BuilderLoggerFactory.CreateLogger<object>();
            var profilingLogger = new ProfilingLogger(logger, profiler);

            AppDomain.CurrentDomain.SetData("DataDirectory", hostingEnvironment?.MapPathContentRoot(Constants.SystemDirectories.Data));

            // application environment
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                logger.LogError(exception, msg);
            };

            // TODO: Don't do this, UmbracoBuilder ctor should handle it...
            builder.TypeLoader = new TypeLoader(typeFinder, appCaches.RuntimeCache,
                new DirectoryInfo(hostingEnvironment.LocalTempPath),
                builder.BuilderLoggerFactory.CreateLogger<TypeLoader>(), profilingLogger);

            builder.Services.AddUnique<IUmbracoBootPermissionChecker>(new AspNetCoreBootPermissionsChecker());
            builder.Services.AddUnique<IProfiler>(profiler);
            builder.Services.AddUnique<IProfilingLogger>(profilingLogger);
            builder.Services.AddUnique<IMainDom>(mainDom);
            builder.Services.AddUnique<AppCaches>(appCaches);
            builder.Services.AddUnique<IRequestCache>(appCaches.RequestCache);
            builder.Services.AddUnique<TypeLoader>(builder.TypeLoader);
            builder.Services.AddUnique<ITypeFinder>(typeFinder);
            builder.Services.AddUnique<IIOHelper>(ioHelper);
            builder.Services.AddUnique<IUmbracoVersion>(umbracoVersion);
            builder.Services.AddUnique<IDbProviderFactoryCreator>(dbProviderFactoryCreator);
            builder.Services.AddUnique<IHostingEnvironment>(hostingEnvironment);
            builder.Services.AddUnique<IRuntime, CoreRuntime>();

            // after bootstrapping we let the container wire up for us.
            builder.Services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            builder.Services.AddUnique<ISqlContext>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
            builder.Services.AddUnique<IBulkSqlInsertProvider>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);

            // re-create the state object with the essential services
            builder.Services.AddUnique<IRuntimeState, RuntimeState>();
        }


        /// <summary>
        /// Create and configure the logger
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public static IServiceCollection AddLogger(
            this IServiceCollection services,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
        {
            // Create a serilog logger
            var logger = SerilogLogger.CreateWithDefaultConfiguration(loggingConfiguration, configuration);

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
