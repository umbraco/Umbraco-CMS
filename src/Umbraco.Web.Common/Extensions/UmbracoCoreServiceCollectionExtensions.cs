using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        /// <returns></returns>
        public static IUmbracoBuilder AddUmbracoCore(
            this IUmbracoBuilder builder,
            IWebHostEnvironment webHostEnvironment,
            Assembly entryAssembly,
            AppCaches appCaches,
            ILoggingConfiguration loggingConfiguration,
            IConfiguration configuration)
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

            builder.Services.AddUnique<IIOHelper, IOHelper>();
            builder.Services.AddLogger(loggingConfiguration, configuration); // TODO: remove this line
            var loggerFactory = builder.BuilderLoggerFactory;

            var profiler = GetWebProfiler(configuration);
            builder.Services.AddUnique<IProfiler>(profiler);
           
            var profilingLogger = new ProfilingLogger(builder.BuilderLoggerFactory.CreateLogger<ProfilingLogger>(), profiler);
            builder.Services.AddUnique<IProfilingLogger>(profilingLogger);

            var typeFinder = CreateTypeFinder(loggerFactory, webHostEnvironment, entryAssembly, builder.Config);
            builder.Services.AddUnique<ITypeFinder>(typeFinder);

            var typeLoader = CreateTypeLoader(typeFinder, webHostEnvironment, loggerFactory, profilingLogger, appCaches.RuntimeCache, configuration);
            builder.TypeLoader = typeLoader;
            builder.Services.AddUnique<TypeLoader>(typeLoader);

            builder.Services.AddUnique<IBackOfficeInfo, AspNetCoreBackOfficeInfo>();

            // after bootstrapping we let the container wire up for us.
            builder.Services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            builder.Services.AddUnique<ISqlContext>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
            builder.Services.AddUnique<IBulkSqlInsertProvider>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);

            builder.Services.AddUnique<AppCaches>(appCaches);
            builder.Services.AddUnique<IRequestCache>(appCaches.RequestCache);
            builder.Services.AddUnique<IUmbracoVersion, UmbracoVersion>();

            builder.Services.AddUnique<IDbProviderFactoryCreator>(dbProviderFactoryCreator);
            builder.Services.AddUnique<IRuntime, CoreRuntime>();
            builder.Services.AddUnique<IRuntimeState, RuntimeState>();
            builder.Services.AddUnique<IHostingEnvironment, AspNetCoreHostingEnvironment>();

            builder.Services.AddUnique<IMainDomLock>(factory =>
            {
                var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var connectionStrings = factory.GetRequiredService<IOptions<ConnectionStrings>>().Value;
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

                var dbCreator = factory.GetRequiredService<IDbProviderFactoryCreator>()
                    ; var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                return globalSettings.MainDomLock.Equals("SqlMainDomLock") || isWindows == false
                    ? (IMainDomLock)new SqlMainDomLock(builder.BuilderLoggerFactory.CreateLogger<SqlMainDomLock>(), builder.BuilderLoggerFactory, globalSettings, connectionStrings, dbCreator, hostingEnvironment)
                    : new MainDomSemaphoreLock(builder.BuilderLoggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);
            });

            builder.Services.AddUnique<IMainDom, MainDom>();

            builder.Services.AddUnique<IUmbracoBootPermissionChecker>(new AspNetCoreBootPermissionsChecker());

            builder.AddComposers();

            var exceptionLogger = builder.BuilderLoggerFactory.CreateLogger<object>();
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var exception = (Exception)args.ExceptionObject;
                var isTerminating = args.IsTerminating; // always true?

                var msg = "Unhandled exception in AppDomain";
                if (isTerminating) msg += " (terminating)";
                msg += ".";
                exceptionLogger.LogError(exception, msg);
            };

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

        private static IProfiler GetWebProfiler(IConfiguration config)
        {
            var isDebug = config.GetValue<bool>($"{Constants.Configuration.ConfigHosting}:Debug");
            // create and start asap to profile boot
            if (!isDebug)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new VoidProfiler();
            }

            var webProfiler = new WebProfiler();
            webProfiler.StartBoot();

            return webProfiler;
        }

        private static ITypeFinder CreateTypeFinder(ILoggerFactory loggerFactory, IWebHostEnvironment webHostEnvironment, Assembly entryAssembly, IConfiguration config)
        {
            var profiler = GetWebProfiler(config);

            var typeFinderSettings = config.GetSection(Core.Constants.Configuration.ConfigTypeFinder).Get<TypeFinderSettings>() ?? new TypeFinderSettings();

            var runtimeHashPaths = new RuntimeHashPaths().AddFolder(new DirectoryInfo(Path.Combine(webHostEnvironment.ContentRootPath, "bin")));
            var runtimeHash = new RuntimeHash(new ProfilingLogger(loggerFactory.CreateLogger("RuntimeHash"), profiler), runtimeHashPaths);

            return new TypeFinder(
                loggerFactory.CreateLogger<TypeFinder>(),
                new DefaultUmbracoAssemblyProvider(entryAssembly),
                runtimeHash,
                new TypeFinderConfig(Options.Create(typeFinderSettings))
            );
        }

        private static TypeLoader CreateTypeLoader(
            ITypeFinder typeFinder,
            IWebHostEnvironment webHostEnvironment,
            ILoggerFactory loggerFactory,
            IProfilingLogger profilingLogger,
            IAppPolicyCache runtimeCache,
            IConfiguration configuration)
        {
            var hostingSettings = configuration.GetSection(Core.Constants.Configuration.ConfigHosting).Get<HostingSettings>() ?? new HostingSettings();
            var hostingEnvironment = new AspNetCoreHostingEnvironmentWithoutOptionsMonitor(hostingSettings, webHostEnvironment);

            return new TypeLoader(
                typeFinder,
                runtimeCache,
                new DirectoryInfo(hostingEnvironment.LocalTempPath),
                loggerFactory.CreateLogger<TypeLoader>(),
                profilingLogger
            );
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
