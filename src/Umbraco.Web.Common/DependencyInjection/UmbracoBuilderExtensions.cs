using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Dazinator.Extensions.FileProviders.GlobPatternFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;
using Smidge;
using Smidge.FileProcessors;
using Smidge.InMemory;
using Smidge.Nuglify;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.ApplicationModels;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Cms.Web.Common.Install;
using Umbraco.Cms.Web.Common.Localization;
using Umbraco.Cms.Web.Common.Macros;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.ModelBinders;
using Umbraco.Cms.Web.Common.Mvc;
using Umbraco.Cms.Web.Common.Profiler;
using Umbraco.Cms.Web.Common.RuntimeMinification;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Common.Templates;
using Umbraco.Cms.Web.Common.UmbracoContext;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Extensions
{
    // TODO: We could add parameters to configure each of these for flexibility

    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Creates an <see cref="IUmbracoBuilder"/> and registers basic Umbraco services
        /// </summary>
        public static IUmbracoBuilder AddUmbraco(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration config)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            IHostingEnvironment tempHostingEnvironment = GetTemporaryHostingEnvironment(webHostEnvironment, config);

            var loggingDir = tempHostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.LogFiles);
            var loggingConfig = new LoggingConfiguration(loggingDir);

            services.AddLogger(tempHostingEnvironment, loggingConfig, config);

            // Manually create and register the HttpContextAccessor. In theory this should not be registered
            // again by the user but if that is the case it's not the end of the world since HttpContextAccessor
            // is just based on AsyncLocal, see https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http/src/HttpContextAccessor.cs
            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton(httpContextAccessor);

            var requestCache = new HttpContextRequestAppCache(httpContextAccessor);
            var appCaches = AppCaches.Create(requestCache);
            services.AddUnique(appCaches);

            IProfiler profiler = GetWebProfiler(config);
            services.AddUnique(profiler);

            ILoggerFactory loggerFactory = LoggerFactory.Create(cfg => cfg.AddSerilog(Log.Logger, false));
            TypeLoader typeLoader = services.AddTypeLoader(Assembly.GetEntryAssembly(), webHostEnvironment, tempHostingEnvironment, loggerFactory, appCaches, config, profiler);

            // adds the umbraco startup filter which will call UseUmbraco early on before
            // other start filters are applied (depending on the ordering of IStartupFilters in DI).
            services.AddTransient<IStartupFilter, UmbracoApplicationServicesCapture>();

            return new UmbracoBuilder(services, config, typeLoader, loggerFactory);
        }

        /// <summary>
        /// Adds core Umbraco services
        /// </summary>
        /// <remarks>
        /// This will not add any composers/components
        /// </remarks>
        public static IUmbracoBuilder AddUmbracoCore(this IUmbracoBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Add ASP.NET specific services
            builder.Services.AddUnique<IBackOfficeInfo, AspNetCoreBackOfficeInfo>();
            builder.Services.AddUnique<IHostingEnvironment, AspNetCoreHostingEnvironment>();
            builder.Services.AddHostedService(factory => factory.GetRequiredService<IRuntime>());

            // Add supported databases
            builder.AddUmbracoSqlServerSupport();
            builder.AddUmbracoSqlCeSupport();
            builder.Services.AddUnique<DatabaseSchemaCreatorFactory>();

            // Must be added here because DbProviderFactories is netstandard 2.1 so cannot exist in Infra for now
            builder.Services.AddSingleton<IDbProviderFactoryCreator>(factory => new DbProviderFactoryCreator(
                DbProviderFactories.GetFactory,
                factory.GetServices<ISqlSyntaxProvider>(),
                factory.GetServices<IBulkSqlInsertProvider>(),
                factory.GetServices<IEmbeddedDatabaseCreator>()
            ));

            builder.AddCoreInitialServices();

            // aspnet app lifetime mgmt
            builder.Services.AddUnique<IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
            builder.Services.AddUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            return builder;
        }

        /// <summary>
        /// Add Umbraco hosted services
        /// </summary>
        public static IUmbracoBuilder AddHostedServices(this IUmbracoBuilder builder)
        {
            builder.Services.AddHostedService<QueuedHostedService>();
            builder.Services.AddHostedService<HealthCheckNotifier>();
            builder.Services.AddHostedService<KeepAlive>();
            builder.Services.AddHostedService<LogScrubber>();
            builder.Services.AddHostedService<ScheduledPublishing>();
            builder.Services.AddHostedService<TempFileCleanup>();
            builder.Services.AddHostedService<InstructionProcessTask>();
            builder.Services.AddHostedService<TouchServerTask>();
            builder.Services.AddHostedService<ReportSiteTask>();
            return builder;
        }

        private static IUmbracoBuilder AddHttpClients(this IUmbracoBuilder builder)
        {
            builder.Services.AddHttpClient();
            return builder;
        }

        /// <summary>
        /// Adds the Umbraco request profiler
        /// </summary>
        public static IUmbracoBuilder AddUmbracoProfiler(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<WebProfilerHtml>();

            builder.Services.AddMiniProfiler(options =>
            {
                // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
                options.ShouldProfile = request => false;

                // this is a default path and by default it performs a 'contains' check which will match our content controller
                // (and probably other requests) and ignore them.
                options.IgnoredPaths.Remove("/content/");
            });

            builder.AddNotificationHandler<UmbracoApplicationStarting, InitializeWebProfiling>();
            return builder;
        }

        public static IUmbracoBuilder AddMvcAndRazor(this IUmbracoBuilder builder, Action<IMvcBuilder> mvcBuilding = null)
        {
            // TODO: We need to figure out if we can work around this because calling AddControllersWithViews modifies the global app and order is very important
            // this will directly affect developers who need to call that themselves.
            // We need to have runtime compilation of views when using umbraco. We could consider having only this when a specific config is set.
            // But as far as I can see, there are still precompiled views, even when this is activated, so maybe it is okay.
            IMvcBuilder mvcBuilder = builder.Services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            mvcBuilding?.Invoke(mvcBuilder);

            return builder;
        }

        /// <summary>
        /// Add runtime minifier support for Umbraco
        /// </summary>
        public static IUmbracoBuilder AddRuntimeMinifier(this IUmbracoBuilder builder)
        {
            // Add custom ISmidgeFileProvider to include the additional App_Plugins location
            // to load assets from.
            builder.Services.AddSingleton<ISmidgeFileProvider>(f =>
            {
                IWebHostEnvironment hostEnv = f.GetRequiredService<IWebHostEnvironment>();

                return new SmidgeFileProvider(
                    hostEnv.WebRootFileProvider,
                    new GlobPatternFilterFileProvider(
                        hostEnv.ContentRootFileProvider,
                        // only include js or css files within App_Plugins
                        new[] { "/App_Plugins/**/*.js", "/App_Plugins/**/*.css" }));
            });

            builder.Services.AddSmidge(builder.Config.GetSection(Constants.Configuration.ConfigRuntimeMinification));
            builder.Services.AddSmidgeNuglify();
            builder.Services.AddSmidgeInMemory(false); // it will be enabled based on config/cachebuster

            builder.Services.AddUnique<IRuntimeMinifier, SmidgeRuntimeMinifier>();
            builder.Services.AddUnique<SmidgeHelperAccessor>();
            builder.Services.AddTransient<IPreProcessor, SmidgeNuglifyJs>();
            builder.Services.ConfigureOptions<SmidgeOptionsSetup>();

            return builder;
        }

        /// <summary>
        /// Adds all web based services required for Umbraco to run
        /// </summary>
        public static IUmbracoBuilder AddWebComponents(this IUmbracoBuilder builder)
        {
            // Add service session
            // This can be overwritten by the user by adding their own call to AddSession
            // since the last call of AddSession take precedence
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = "UMB_SESSION";
                options.Cookie.HttpOnly = true;
            });

            builder.Services.ConfigureOptions<UmbracoMvcConfigureOptions>();
            builder.Services.ConfigureOptions<UmbracoRequestLocalizationOptions>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, UmbracoApiBehaviorApplicationModelProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, BackOfficeApplicationModelProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, VirtualPageApplicationModelProvider>());
            builder.AddUmbracoImageSharp();

            // AspNetCore specific services
            builder.Services.AddUnique<IRequestAccessor, AspNetCoreRequestAccessor>();
            builder.AddNotificationHandler<UmbracoRequestBegin, AspNetCoreRequestAccessor>();

            // Password hasher
            builder.Services.AddUnique<IPasswordHasher, AspNetCorePasswordHasher>();

            builder.Services.AddUnique<Cms.Core.Web.ICookieManager, AspNetCoreCookieManager>();
            builder.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
            builder.Services.AddUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            builder.Services.AddMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            builder.Services.AddUnique<IMarchal, AspNetCoreMarchal>();

            builder.Services.AddUnique<IProfilerHtml, WebProfilerHtml>();

            builder.Services.AddUnique<IMacroRenderer, MacroRenderer>();
            builder.Services.AddUnique<PartialViewMacroEngine>();

            // register the umbraco context factory

            builder.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            builder.Services.AddUnique<IBackOfficeSecurityAccessor, BackOfficeSecurityAccessor>();

            var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
            builder.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);

            builder.Services.AddUnique<InstallAreaRoutes>();

            builder.Services.AddUnique<UmbracoRequestLoggingMiddleware>();
            builder.Services.AddUnique<PreviewAuthenticationMiddleware>();
            builder.Services.AddUnique<UmbracoRequestMiddleware>();
            builder.Services.AddUnique<BootFailedMiddleware>();

            builder.Services.AddUnique<UmbracoJsonModelBinder>();

            builder.Services.AddUnique<ITemplateRenderer, TemplateRenderer>();
            builder.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();

            builder.Services.AddSingleton<ContentModelBinder>();

            builder.Services.AddScoped<UmbracoHelper>();
            builder.Services.AddScoped<IBackOfficeSecurity, BackOfficeSecurity>();

            builder.AddHttpClients();

            // TODO: Does this belong in web components??
            builder.AddNuCache();

            return builder;
        }

        public static IUmbracoBuilder AddUnattedInstallCreateUser(this IUmbracoBuilder builder)
        {
            builder.AddNotificationAsyncHandler<UnattendedInstallNotification, CreateUnattendedUserNotificationHandler>();
            return builder;
        }

        // TODO: Does this need to exist and/or be public?
        public static IUmbracoBuilder AddWebServer(this IUmbracoBuilder builder)
        {
            // TODO: We need to figure out why this is needed and fix those endpoints to not need them, we don't want to change global things
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
        private static IUmbracoBuilder AddUmbracoSqlCeSupport(this IUmbracoBuilder builder)
        {
            try
            {
                var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (binFolder != null)
                {
                    var dllPath = Path.Combine(binFolder, "Umbraco.Persistence.SqlCe.dll");
                    var umbSqlCeAssembly = Assembly.LoadFrom(dllPath);

                    var sqlCeSyntaxProviderType = umbSqlCeAssembly.GetType("Umbraco.Cms.Persistence.SqlCe.SqlCeSyntaxProvider");
                    var sqlCeBulkSqlInsertProviderType = umbSqlCeAssembly.GetType("Umbraco.Cms.Persistence.SqlCe.SqlCeBulkSqlInsertProvider");
                    var sqlCeEmbeddedDatabaseCreatorType = umbSqlCeAssembly.GetType("Umbraco.Cms.Persistence.SqlCe.SqlCeEmbeddedDatabaseCreator");

                    if (!(sqlCeSyntaxProviderType is null || sqlCeBulkSqlInsertProviderType is null || sqlCeEmbeddedDatabaseCreatorType is null))
                    {
                        builder.Services.AddSingleton(typeof(ISqlSyntaxProvider), sqlCeSyntaxProviderType);
                        builder.Services.AddSingleton(typeof(IBulkSqlInsertProvider), sqlCeBulkSqlInsertProviderType);
                        builder.Services.AddSingleton(typeof(IEmbeddedDatabaseCreator), sqlCeEmbeddedDatabaseCreatorType);
                    }

                    var sqlCeAssembly = Assembly.LoadFrom(Path.Combine(binFolder, "System.Data.SqlServerCe.dll"));

                    var sqlCe = sqlCeAssembly.GetType("System.Data.SqlServerCe.SqlCeProviderFactory");
                    if (!(sqlCe is null))
                    {
                        DbProviderFactories.RegisterFactory(Cms.Core.Constants.DbProviderNames.SqlCe, sqlCe);
                    }
                }
            }
            catch
            {
                // Ignore if SqlCE is not available
            }

            return builder;
        }

        /// <summary>
        /// Adds Sql Server support for Umbraco
        /// </summary>
        private static IUmbracoBuilder AddUmbracoSqlServerSupport(this IUmbracoBuilder builder)
        {
            DbProviderFactories.RegisterFactory(Cms.Core.Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            builder.Services.AddSingleton<ISqlSyntaxProvider, SqlServerSyntaxProvider>();
            builder.Services.AddSingleton<IBulkSqlInsertProvider, SqlServerBulkSqlInsertProvider>();
            builder.Services.AddSingleton<IEmbeddedDatabaseCreator, NoopEmbeddedDatabaseCreator>();

            return builder;
        }

        private static IProfiler GetWebProfiler(IConfiguration config)
        {
            var isDebug = config.GetValue<bool>($"{Cms.Core.Constants.Configuration.ConfigHosting}:Debug");
            // create and start asap to profile boot
            if (!isDebug)
            {
                // should let it be null, that's how MiniProfiler is meant to work,
                // but our own IProfiler expects an instance so let's get one
                return new NoopProfiler();
            }

            var webProfiler = new WebProfiler();
            webProfiler.StartBoot();

            return webProfiler;
        }

        /// <summary>
        /// HACK: returns an AspNetCoreHostingEnvironment that doesn't monitor changes to configuration.<br/>
        /// We require this to create a TypeLoader during ConfigureServices.<br/>
        /// Instances returned from this method shouldn't be registered in the service collection.
        /// </summary>
        private static IHostingEnvironment GetTemporaryHostingEnvironment(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            var hostingSettings = config.GetSection(Cms.Core.Constants.Configuration.ConfigHosting).Get<HostingSettings>() ?? new HostingSettings();
            var webRoutingSettings = config.GetSection(Cms.Core.Constants.Configuration.ConfigWebRouting).Get<WebRoutingSettings>() ?? new WebRoutingSettings();
            var wrappedHostingSettings = new OptionsMonitorAdapter<HostingSettings>(hostingSettings);
            var wrappedWebRoutingSettings = new OptionsMonitorAdapter<WebRoutingSettings>(webRoutingSettings);

            return new AspNetCoreHostingEnvironment(wrappedHostingSettings, wrappedWebRoutingSettings, webHostEnvironment);
        }

    }
}
