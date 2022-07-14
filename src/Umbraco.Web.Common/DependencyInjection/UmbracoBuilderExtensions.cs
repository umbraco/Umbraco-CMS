using System.Data.Common;
using System.Reflection;
using Dazinator.Extensions.FileProviders.GlobPatternFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Extensions.Logging;
using Smidge;
using Smidge.Cache;
using Smidge.FileProcessors;
using Smidge.InMemory;
using Smidge.Nuglify;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Diagnostics;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Macros;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;
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

namespace Umbraco.Extensions;

// TODO: We could add parameters to configure each of these for flexibility

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the common Umbraco functionality
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Creates an <see cref="IUmbracoBuilder" /> and registers basic Umbraco services
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

        // Setup static application logging ASAP (e.g. during configure services).
        // Will log to SilentLogger until Serilog.Log.Logger is setup.
        StaticApplicationLogging.Initialize(new SerilogLoggerFactory());

        // The DataDirectory is used to resolve database file paths (directly supported by SQL CE and manually replaced for LocalDB)
        AppDomain.CurrentDomain.SetData(
            "DataDirectory",
            webHostEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data));

        // Manually create and register the HttpContextAccessor. In theory this should not be registered
        // again by the user but if that is the case it's not the end of the world since HttpContextAccessor
        // is just based on AsyncLocal, see https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http/src/HttpContextAccessor.cs
        IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
        services.AddSingleton(httpContextAccessor);

        var requestCache = new HttpContextRequestAppCache(httpContextAccessor);
        var appCaches = AppCaches.Create(requestCache);

        services.ConfigureOptions<ConfigureKestrelServerOptions>();
        services.ConfigureOptions<ConfigureFormOptions>();

        IProfiler profiler = GetWebProfiler(config);

        services.AddLogger(webHostEnvironment, config);

        ILoggerFactory loggerFactory = new SerilogLoggerFactory();

        TypeLoader typeLoader = services.AddTypeLoader(Assembly.GetEntryAssembly(), loggerFactory, config);

        IHostingEnvironment tempHostingEnvironment = GetTemporaryHostingEnvironment(webHostEnvironment, config);
        return new UmbracoBuilder(services, config, typeLoader, loggerFactory, profiler, appCaches, tempHostingEnvironment);
    }

    /// <summary>
    ///     Adds core Umbraco services
    /// </summary>
    /// <remarks>
    ///     This will not add any composers/components
    /// </remarks>
    public static IUmbracoBuilder AddUmbracoCore(this IUmbracoBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        // Add ASP.NET specific services
        builder.Services.AddUnique<IBackOfficeInfo, AspNetCoreBackOfficeInfo>();
        builder.Services.AddUnique<IHostingEnvironment>(sp =>
            ActivatorUtilities.CreateInstance<AspNetCoreHostingEnvironment>(
                sp,
                sp.GetRequiredService<IApplicationDiscriminator>()));

        builder.Services.AddHostedService(factory => factory.GetRequiredService<IRuntime>());

        builder.Services.AddSingleton<DatabaseSchemaCreatorFactory>();
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDatabaseProviderMetadata, CustomConnectionStringDatabaseProviderMetadata>());

        // Must be added here because DbProviderFactories is netstandard 2.1 so cannot exist in Infra for now
        builder.Services.AddSingleton<IDbProviderFactoryCreator>(factory => new DbProviderFactoryCreator(
            DbProviderFactories.GetFactory,
            factory.GetServices<ISqlSyntaxProvider>(),
            factory.GetServices<IBulkSqlInsertProvider>(),
            factory.GetServices<IDatabaseCreator>(),
            factory.GetServices<IProviderSpecificMapperFactory>(),
            factory.GetServices<IProviderSpecificInterceptor>()));

        builder.AddCoreInitialServices();
        builder.AddTelemetryProviders();

        // aspnet app lifetime mgmt
        builder.Services.AddUnique<IUmbracoApplicationLifetime, AspNetCoreUmbracoApplicationLifetime>();
        builder.Services.AddUnique<IApplicationShutdownRegistry, AspNetCoreApplicationShutdownRegistry>();
        builder.Services.AddTransient<IIpAddressUtilities, IpAddressUtilities>();

        return builder;
    }

    /// <summary>
    ///     Add Umbraco hosted services
    /// </summary>
    public static IUmbracoBuilder AddHostedServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddHostedService<QueuedHostedService>();
        builder.Services.AddHostedService<HealthCheckNotifier>();
        builder.Services.AddHostedService<KeepAlive>();
        builder.Services.AddHostedService<LogScrubber>();
        builder.Services.AddHostedService<ContentVersionCleanup>();
        builder.Services.AddHostedService<ScheduledPublishing>();
        builder.Services.AddHostedService<TempFileCleanup>();
        builder.Services.AddHostedService<InstructionProcessTask>();
        builder.Services.AddHostedService<TouchServerTask>();
        builder.Services.AddHostedService(provider =>
            new ReportSiteTask(
                provider.GetRequiredService<ILogger<ReportSiteTask>>(),
                provider.GetRequiredService<ITelemetryService>()));
        return builder;
    }

    /// <summary>
    ///     Adds the Umbraco request profiler
    /// </summary>
    public static IUmbracoBuilder AddUmbracoProfiler(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<WebProfilerHtml>();

        builder.Services.AddMiniProfiler(options =>
        {
            // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            options.ShouldProfile = request => false;

            // this is a default path and by default it performs a 'contains' check which will match our content controller
            // (and probably other requests) and ignore them.
            options.IgnoredPaths.Remove("/content/");
        });

        builder.AddNotificationHandler<UmbracoApplicationStartingNotification, InitializeWebProfiling>();
        return builder;
    }

    private static IUmbracoBuilder AddHttpClients(this IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient(Constants.HttpClients.IgnoreCertificateErrors)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            });
        return builder;
    }

    public static IUmbracoBuilder AddMvcAndRazor(this IUmbracoBuilder builder, Action<IMvcBuilder>? mvcBuilding = null)
    {
        // TODO: We need to figure out if we can work around this because calling AddControllersWithViews modifies the global app and order is very important
        // this will directly affect developers who need to call that themselves.
        IMvcBuilder mvcBuilder = builder.Services.AddControllersWithViews();

        if (builder.Config.GetRuntimeMode() != RuntimeMode.Production)
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        mvcBuilding?.Invoke(mvcBuilder);

        return builder;
    }

    /// <summary>
    ///     Add runtime minifier support for Umbraco
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

        builder.Services.AddUnique<ICacheBuster, UmbracoSmidgeConfigCacheBuster>();
        builder.Services.AddSmidge(builder.Config.GetSection(Constants.Configuration.ConfigRuntimeMinification));

        // Replace the Smidge request helper, in order to discourage the use of brotli since it's super slow
        builder.Services.AddUnique<IRequestHelper, SmidgeRequestHelper>();
        builder.Services.AddSmidgeNuglify();
        builder.Services.AddSmidgeInMemory(false); // it will be enabled based on config/cachebuster

        builder.Services.AddUnique<IRuntimeMinifier, SmidgeRuntimeMinifier>();
        builder.Services.AddSingleton<SmidgeHelperAccessor>();
        builder.Services.AddTransient<IPreProcessor, SmidgeNuglifyJs>();
        builder.Services.ConfigureOptions<SmidgeOptionsSetup>();

        return builder;
    }

    /// <summary>
    ///     Adds all web based services required for Umbraco to run
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
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Transient<IApplicationModelProvider, UmbracoApiBehaviorApplicationModelProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Transient<IApplicationModelProvider, BackOfficeApplicationModelProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Transient<IApplicationModelProvider, VirtualPageApplicationModelProvider>());
        builder.AddUmbracoImageSharp();

        // AspNetCore specific services
        builder.Services.AddUnique<IRequestAccessor, AspNetCoreRequestAccessor>();
        builder.AddNotificationHandler<UmbracoRequestBeginNotification, AspNetCoreRequestAccessor>();

        // Password hasher
        builder.Services.AddUnique<IPasswordHasher, AspNetCorePasswordHasher>();

        builder.Services.AddUnique<ICookieManager, AspNetCoreCookieManager>();
        builder.Services.AddTransient<IIpResolver, AspNetCoreIpResolver>();
        builder.Services.AddUnique<IUserAgentProvider, AspNetCoreUserAgentProvider>();

        builder.Services.AddMultipleUnique<ISessionIdResolver, ISessionManager, AspNetCoreSessionManager>();

        builder.Services.AddUnique<IMarchal, AspNetCoreMarchal>();

        builder.Services.AddUnique<IProfilerHtml, WebProfilerHtml>();

        builder.Services.AddUnique<IMacroRenderer>(serviceProvider =>
        {
            return new MacroRenderer(
                serviceProvider.GetRequiredService<IProfilingLogger>(),
                serviceProvider.GetRequiredService<ILogger<MacroRenderer>>(),
                serviceProvider.GetRequiredService<IUmbracoContextAccessor>(),
                serviceProvider.GetRequiredService<IOptionsMonitor<ContentSettings>>(),
                serviceProvider.GetRequiredService<ILocalizedTextService>(),
                serviceProvider.GetRequiredService<AppCaches>(),
                serviceProvider.GetRequiredService<IMacroService>(),
                serviceProvider.GetRequiredService<ICookieManager>(),
                serviceProvider.GetRequiredService<ISessionManager>(),
                serviceProvider.GetRequiredService<IRequestAccessor>(),
                serviceProvider.GetRequiredService<PartialViewMacroEngine>(),
                serviceProvider.GetRequiredService<IHttpContextAccessor>(),
                serviceProvider.GetRequiredService<IWebHostEnvironment>());
        });

        builder.Services.AddSingleton<PartialViewMacroEngine>();

        // register the umbraco context factory
        builder.Services.AddUnique<IUmbracoContextFactory, UmbracoContextFactory>();
        builder.Services.AddUnique<IBackOfficeSecurityAccessor, BackOfficeSecurityAccessor>();

        var umbracoApiControllerTypes = builder.TypeLoader.GetUmbracoApiControllers().ToList();
        builder.WithCollectionBuilder<UmbracoApiControllerTypeCollectionBuilder>()
            .Add(umbracoApiControllerTypes);

        builder.Services.AddSingleton<UmbracoRequestLoggingMiddleware>();
        builder.Services.AddSingleton<PreviewAuthenticationMiddleware>();
        builder.Services.AddSingleton<UmbracoRequestMiddleware>();
        builder.Services.AddSingleton<BootFailedMiddleware>();

        builder.Services.AddSingleton<UmbracoJsonModelBinder>();

        builder.Services.AddUnique<ITemplateRenderer, TemplateRenderer>();
        builder.Services.AddUnique<IPublicAccessChecker, PublicAccessChecker>();

        builder.Services.AddSingleton<ContentModelBinder>();

        builder.Services.AddSingleton<IUmbracoHelperAccessor, UmbracoHelperAccessor>();
        builder.Services.AddSingleton<IScopedServiceProvider, ScopedServiceProvider>();
        builder.Services.AddScoped<UmbracoHelper>();
        builder.Services.AddScoped<IBackOfficeSecurity, BackOfficeSecurity>();

        builder.AddHttpClients();

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

    private static IProfiler GetWebProfiler(IConfiguration config)
    {
        var isDebug = config.GetValue<bool>($"{Constants.Configuration.ConfigHosting}:Debug");

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
    ///     HACK: returns an AspNetCoreHostingEnvironment that doesn't monitor changes to configuration.<br />
    ///     We require this to create a TypeLoader during ConfigureServices.<br />
    ///     Instances returned from this method shouldn't be registered in the service collection.
    /// </summary>
    private static IHostingEnvironment GetTemporaryHostingEnvironment(
        IWebHostEnvironment webHostEnvironment,
        IConfiguration config)
    {
        HostingSettings hostingSettings =
            config.GetSection(Constants.Configuration.ConfigHosting).Get<HostingSettings>() ?? new HostingSettings();
        var wrappedHostingSettings = new OptionsMonitorAdapter<HostingSettings>(hostingSettings);

        WebRoutingSettings webRoutingSettings =
            config.GetSection(Constants.Configuration.ConfigWebRouting).Get<WebRoutingSettings>() ??
            new WebRoutingSettings();
        var wrappedWebRoutingSettings = new OptionsMonitorAdapter<WebRoutingSettings>(webRoutingSettings);

        return new AspNetCoreHostingEnvironment(
            wrappedHostingSettings,
            wrappedWebRoutingSettings,
            webHostEnvironment);
    }
}
