using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Smidge;
using Smidge.Nuglify;
using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Runtime;
using Umbraco.Extensions;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Web.Common.ApplicationModels;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.ModelBinders;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Web.Common.Builder
{
    // TODO: We could add parameters to configure each of these for flexibility
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbraco(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration config)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (config is null) throw new ArgumentNullException(nameof(config));

            var loggingConfig = new LoggingConfiguration(Path.Combine(webHostEnvironment.ContentRootPath, "umbraco", "logs"));
            services.AddSingleton<ILoggingConfiguration>(loggingConfig);
            services.AddLogger(loggingConfig, config);

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton(httpContextAccessor);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext?.Items);
            var appCaches = AppCaches.Create(requestCache);
            services.AddUnique<AppCaches>(appCaches);

            var loggerFactory = LoggerFactory.Create(cfg => cfg.AddSerilog(Log.Logger, false));
            var typeLoader = services.AddTypeLoader(Assembly.GetEntryAssembly(), webHostEnvironment, loggerFactory, appCaches, config);

            return new UmbracoBuilder(services, config, typeLoader, loggerFactory);
        }

        /// <remarks>Composes Composers</remarks>
        public static IUmbracoBuilder AddUmbracoCore(this IUmbracoBuilder builder)
        {
            if (builder is null) throw new ArgumentNullException(nameof(builder));

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

            builder.Services.AddUnique<IAppPolicyCache>(factory => factory.GetRequiredService<AppCaches>().RuntimeCache);
            builder.Services.AddUnique<IRequestCache>(factory => factory.GetRequiredService<AppCaches>().RequestCache);

            var dbProviderFactoryCreator = new DbProviderFactoryCreator(
                DbProviderFactories.GetFactory,
                syntaxProviders,
                insertProviders,
                databaseCreators);

            builder.Services.AddSingleton<IDbProviderFactoryCreator>(dbProviderFactoryCreator);

            builder.Services.AddUnique<IIOHelper, IOHelper>();

            var profiler = UmbracoCoreServiceCollectionExtensions.GetWebProfiler(builder.Config);
            builder.Services.AddUnique<IProfiler>(profiler);

            builder.Services.AddUnique<IProfilingLogger, ProfilingLogger>();
            builder.Services.AddUnique<TypeLoader>(builder.TypeLoader);
            builder.Services.AddUnique<IBackOfficeInfo, AspNetCoreBackOfficeInfo>();

            builder.Services.AddUnique<IUmbracoDatabaseFactory, UmbracoDatabaseFactory>();
            builder.Services.AddUnique<ISqlContext>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().SqlContext);
            builder.Services.AddUnique<IBulkSqlInsertProvider>(factory => factory.GetRequiredService<IUmbracoDatabaseFactory>().BulkSqlInsertProvider);

            builder.Services.AddUnique<IUmbracoVersion, UmbracoVersion>();

            builder.Services.AddUnique<IDbProviderFactoryCreator>(dbProviderFactoryCreator);
            builder.Services.AddUnique<IRuntime, CoreRuntime>();
            builder.Services.AddUnique<IRuntimeState, RuntimeState>();
            builder.Services.AddUnique<Core.Hosting.IHostingEnvironment, AspNetCoreHostingEnvironment>();

            builder.Services.AddUnique<IMainDomLock>(factory =>
            {
                var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>().Value;
                var connectionStrings = factory.GetRequiredService<IOptions<ConnectionStrings>>().Value;
                var hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();

                var dbCreator = factory.GetRequiredService<IDbProviderFactoryCreator>();
                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                return globalSettings.MainDomLock.Equals("SqlMainDomLock") || isWindows == false
                    ? (IMainDomLock)new SqlMainDomLock(builder.BuilderLoggerFactory.CreateLogger<SqlMainDomLock>(), builder.BuilderLoggerFactory, globalSettings, connectionStrings, dbCreator, hostingEnvironment)
                    : new MainDomSemaphoreLock(builder.BuilderLoggerFactory.CreateLogger<MainDomSemaphoreLock>(), hostingEnvironment);
            });

            builder.Services.AddUnique<IMainDom, MainDom>();

            builder.Services.AddUnique<IUmbracoBootPermissionChecker>(new UmbracoCoreServiceCollectionExtensions.AspNetCoreBootPermissionsChecker());

            builder.AddComposers();

            var exceptionLogger = builder.BuilderLoggerFactory.CreateLogger<object>();

            // This probably shouldn't live here.
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

        public static IUmbracoBuilder AddConfiguration(this IUmbracoBuilder builder)
        {
            // Register configuration validators.
            builder.Services.AddSingleton<IValidateOptions<ContentSettings>, ContentSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<GlobalSettings>, GlobalSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<HealthChecksSettings>, HealthChecksSettingsValidator>();
            builder.Services.AddSingleton<IValidateOptions<RequestHandlerSettings>, RequestHandlerSettingsValidator>();

            // Register configuration sections.
            builder.Services.Configure<ActiveDirectorySettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigActiveDirectory));
            builder.Services.Configure<ConnectionStrings>(builder.Config.GetSection("ConnectionStrings"), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<ContentSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigContent));
            builder.Services.Configure<CoreDebugSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigCoreDebug));
            builder.Services.Configure<ExceptionFilterSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigExceptionFilter));
            builder.Services.Configure<GlobalSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigGlobal));
            builder.Services.Configure<HealthChecksSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigHealthChecks));
            builder.Services.Configure<HostingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigHosting));
            builder.Services.Configure<ImagingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigImaging));
            builder.Services.Configure<IndexCreatorSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigExamine));
            builder.Services.Configure<KeepAliveSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigKeepAlive));
            builder.Services.Configure<LoggingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigLogging));
            builder.Services.Configure<MemberPasswordConfigurationSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigMemberPassword));
            builder.Services.Configure<ModelsBuilderSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigModelsBuilder), o => o.BindNonPublicProperties = true);
            builder.Services.Configure<NuCacheSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigNuCache));
            builder.Services.Configure<RequestHandlerSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigRequestHandler));
            builder.Services.Configure<RuntimeSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigRuntime));
            builder.Services.Configure<SecuritySettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigSecurity));
            builder.Services.Configure<TourSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigTours));
            builder.Services.Configure<TypeFinderSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigTypeFinder));
            builder.Services.Configure<UserPasswordConfigurationSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigUserPassword));
            builder.Services.Configure<WebRoutingSettings>(builder.Config.GetSection(Core.Constants.Configuration.ConfigWebRouting));

            return builder;
        }


        public static IUmbracoBuilder AddHostedServices(this IUmbracoBuilder builder)
        {
            builder.Services.AddHostedService<HealthCheckNotifier>();
            builder.Services.AddHostedService<KeepAlive>();
            builder.Services.AddHostedService<LogScrubber>();
            builder.Services.AddHostedService<ScheduledPublishing>();
            builder.Services.AddHostedService<TempFileCleanup>();
            builder.Services.AddHostedService<InstructionProcessTask>();
            builder.Services.AddHostedService<TouchServerTask>();
            return builder;
        }

        public static IUmbracoBuilder AddHttpClients(this IUmbracoBuilder builder)
        {
            builder.Services.AddHttpClient();
            return builder;
        }

        public static IUmbracoBuilder AddMiniProfiler(this IUmbracoBuilder builder)
        {
            builder.Services.AddMiniProfiler(options =>
            {
                options.ShouldProfile = request => false; // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            });

            return builder;
        }

        public static IUmbracoBuilder AddMvcAndRazor(this IUmbracoBuilder builder, Action<MvcOptions> mvcOptions = null, Action<IMvcBuilder> mvcBuilding = null)
        {
            // TODO: We need to figure out if we can work around this because calling AddControllersWithViews modifies the global app and order is very important
            // this will directly affect developers who need to call that themselves.
            //We need to have runtime compilation of views when using umbraco. We could consider having only this when a specific config is set.
            //But as far as I can see, there are still precompiled views, even when this is activated, so maybe it is okay.
            var mvcBuilder = builder.Services.AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new ContentModelBinderProvider());

                options.Filters.Insert(0, new EnsurePartialViewMacroViewContextFilterAttribute());
                mvcOptions?.Invoke(options);
            }).AddRazorRuntimeCompilation();
            mvcBuilding?.Invoke(mvcBuilder);

            return builder;
        }

        public static IUmbracoBuilder AddRuntimeMinifier(this IUmbracoBuilder builder)
        {
            builder.Services.AddSmidge(builder.Config.GetSection(Core.Constants.Configuration.ConfigRuntimeMinification));
            builder.Services.AddSmidgeNuglify();

            return builder;
        }

        public static IUmbracoBuilder AddWebComponents(this IUmbracoBuilder builder)
        {
            builder.Services.ConfigureOptions<UmbracoWebServiceCollectionExtensions.UmbracoMvcConfigureOptions>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, UmbracoApiBehaviorApplicationModelProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, BackOfficeApplicationModelProvider>());
            builder.Services.AddUmbracoImageSharp(builder.Config);

            return builder;
        }

        public static IUmbracoBuilder AddWebServer(this IUmbracoBuilder builder)
        {
            // TODO: We need to figure out why thsi is needed and fix those endpoints to not need them, we don't want to change global things
            // If using Kestrel: https://stackoverflow.com/a/55196057
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            return builder;
        }

        /// <summary>
        /// Adds SqlCe support for Umbraco
        /// </summary>
        private static void AddUmbracoSqlCeSupport(
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
        }

        /// <summary>
        /// Adds Sql Server support for Umbraco
        /// </summary>
        private static void AddUmbracoSqlServerSupport(
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
        }
    }
}
