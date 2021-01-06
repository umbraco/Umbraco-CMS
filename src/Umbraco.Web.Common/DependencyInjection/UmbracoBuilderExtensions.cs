using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Smidge.Nuglify;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Diagnostics;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Infrastructure.DependencyInjection;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Infrastructure.HostedServices.ServerRegistration;
using Umbraco.Infrastructure.PublishedCache.DependencyInjection;
using Umbraco.Net;
using Umbraco.Web.Common.ApplicationModels;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Install;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Common.Macros;
using Umbraco.Web.Common.Middleware;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.Common.Profiler;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Common.Security;
using Umbraco.Web.Common.Templates;
using Umbraco.Web.Macros;
using Umbraco.Web.Security;
using Umbraco.Web.Telemetry;
using Umbraco.Web.Templates;
using IHostingEnvironment = Umbraco.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Web.Common.DependencyInjection
{
    // TODO: We could add parameters to configure each of these for flexibility

    /// <summary>
    /// Extension methods for <see cref="IUmbracoBuilder"/> for the common Umbraco functionality
    /// </summary>
    public static class UmbracoBuilderExtensions
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

            var loggingDir = tempHostingEnvironment.MapPathContentRoot(Core.Constants.SystemDirectories.LogFiles);
            var loggingConfig = new LoggingConfiguration(loggingDir);

            services.AddLogger(tempHostingEnvironment, loggingConfig, config);

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            services.AddSingleton(httpContextAccessor);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext?.Items);
            var appCaches = AppCaches.Create(requestCache);
            services.AddUnique(appCaches);

            IProfiler profiler = GetWebProfiler(config);
            services.AddUnique(profiler);

            ILoggerFactory loggerFactory = LoggerFactory.Create(cfg => cfg.AddSerilog(Log.Logger, false));
            TypeLoader typeLoader = services.AddTypeLoader(Assembly.GetEntryAssembly(), webHostEnvironment, tempHostingEnvironment, loggerFactory, appCaches, config, profiler);

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


            // Must be added here because DbProviderFactories is netstandard 2.1 so cannot exist in Infra for now
            builder.Services.AddSingleton<IDbProviderFactoryCreator>(factory => new DbProviderFactoryCreator(
                DbProviderFactories.GetFactory,
                factory.GetServices<ISqlSyntaxProvider>(),
                factory.GetServices<IBulkSqlInsertProvider>(),
                factory.GetServices<IEmbeddedDatabaseCreator>()
            ));

            builder.AddCoreInitialServices();

            // aspnet app lifetime mgmt
            builder.Services.AddMultipleUnique<IUmbracoApplicationLifetimeManager, IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
            builder.Services.AddUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();

            return builder;
        }

        /// <summary>
        /// Add Umbraco hosted services
        /// </summary>
        public static IUmbracoBuilder AddHostedServices(this IUmbracoBuilder builder)
        {
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
        /// Adds mini profiler services for Umbraco
        /// </summary>
        public static IUmbracoBuilder AddMiniProfiler(this IUmbracoBuilder builder)
        {
            builder.Services.AddMiniProfiler(options =>

                // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
                options.ShouldProfile = request => false);

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
            builder.Services.AddSmidge(builder.Config.GetSection(Core.Constants.Configuration.ConfigRuntimeMinification));
            builder.Services.AddSmidgeNuglify();

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
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, UmbracoApiBehaviorApplicationModelProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, BackOfficeApplicationModelProvider>());
            builder.Services.AddUmbracoImageSharp(builder.Config);

            // AspNetCore specific services
            builder.Services.AddUnique<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddUnique<IRequestAccessor, AspNetCoreRequestAccessor>();

            // The umbraco request lifetime
            builder.Services.AddMultipleUnique<IUmbracoRequestLifetime, IUmbracoRequestLifetimeManager, UmbracoRequestLifetime>();

            // Password hasher
            builder.Services.AddUnique<IPasswordHasher, AspNetCorePasswordHasher>();

            builder.Services.AddUnique<ICookieManager, AspNetCoreCookieManager>();
            builder.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
            builder.Services.AddUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

            builder.Services.AddMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

            builder.Services.AddUnique<IMarchal, AspNetCoreMarchal>();

            builder.Services.AddUnique<IProfilerHtml, WebProfilerHtml>();

            builder.Services.AddUnique<IMacroRenderer, MacroRenderer>();
            builder.Services.AddUnique<IMemberUserKeyProvider, MemberUserKeyProvider>();

            // register the umbraco context factory

            builder.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
            builder.Services.AddUnique<IBackOfficeSecurityFactory, BackOfficeSecurityFactory>();
            builder.Services.AddUnique<IBackOfficeSecurityAccessor, HybridBackofficeSecurityAccessor>();
            builder.Services.AddUnique<IUmbracoWebsiteSecurityAccessor, HybridUmbracoWebsiteSecurityAccessor>();

            var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
            builder.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
                .Add(umbracoApiControllerTypes);

            builder.Services.AddUnique<InstallAreaRoutes>();

            builder.Services.AddUnique<UmbracoRequestLoggingMiddleware>();
            builder.Services.AddUnique<UmbracoRequestMiddleware>();
            builder.Services.AddUnique<BootFailedMiddleware>();

            builder.Services.AddUnique<UmbracoJsonModelBinder>();

            builder.Services.AddUnique<ITemplateRenderer, TemplateRenderer>();
            builder.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();

            builder.AddHttpClients();

            // TODO: Does this belong in web components??
            builder.AddNuCache();

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
                    var dllPath = Path.Combine(binFolder, "Umbraco.Persistance.SqlCe.dll");
                    var umbSqlCeAssembly = Assembly.LoadFrom(dllPath);

                    var sqlCeSyntaxProviderType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeSyntaxProvider");
                    var sqlCeBulkSqlInsertProviderType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeBulkSqlInsertProvider");
                    var sqlCeEmbeddedDatabaseCreatorType = umbSqlCeAssembly.GetType("Umbraco.Persistance.SqlCe.SqlCeEmbeddedDatabaseCreator");

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
                        DbProviderFactories.RegisterFactory(Core.Constants.DbProviderNames.SqlCe, sqlCe);
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
            DbProviderFactories.RegisterFactory(Core.Constants.DbProviderNames.SqlServer, SqlClientFactory.Instance);

            builder.Services.AddSingleton<ISqlSyntaxProvider, SqlServerSyntaxProvider>();
            builder.Services.AddSingleton<IBulkSqlInsertProvider, SqlServerBulkSqlInsertProvider>();
            builder.Services.AddSingleton<IEmbeddedDatabaseCreator, NoopEmbeddedDatabaseCreator>();

            return builder;
        }

        private static IProfiler GetWebProfiler(IConfiguration config)
        {
            var isDebug = config.GetValue<bool>($"{Core.Constants.Configuration.ConfigHosting}:Debug");
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
            var hostingSettings = config.GetSection(Core.Constants.Configuration.ConfigHosting).Get<HostingSettings>() ?? new HostingSettings();
            var wrappedHostingSettings = new OptionsMonitorAdapter<HostingSettings>(hostingSettings);

            return new AspNetCoreHostingEnvironment(wrappedHostingSettings, webHostEnvironment);
        }
    }
}
