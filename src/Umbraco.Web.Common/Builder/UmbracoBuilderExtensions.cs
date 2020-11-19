using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
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
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.Models.Validation;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.ApplicationModels;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.ModelBinders;

namespace Umbraco.Web.Common.Builder
{
    // TODO: We could add parameters to configure each of these for flexibility
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbraco(
            this IServiceCollection services,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration config,
            ILoggingConfiguration loggingConfig = null,
            ILoggerFactory loggerFactory = null)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (config is null) throw new ArgumentNullException(nameof(config));

            services.AddLazySupport();

            loggingConfig ??= new LoggingConfiguration(Path.Combine(webHostEnvironment.ContentRootPath, "umbraco", "logs"));
            services.AddLogger(loggingConfig, config);
            loggerFactory ??= LoggerFactory.Create(cfg => cfg.AddSerilog(Log.Logger, false));

            return new UmbracoBuilder(services, config, loggerFactory);
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

        public static IUmbracoBuilder AddCore(this IUmbracoBuilder builder, IWebHostEnvironment webHostEnvironment)
        {
            var loggingConfig = new LoggingConfiguration(Path.Combine(webHostEnvironment.ContentRootPath, "umbraco", "logs"));

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            builder.Services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            builder.Services.AddSingleton<ILoggingConfiguration>(loggingConfig);

            var requestCache = new GenericDictionaryRequestAppCache(() => httpContextAccessor.HttpContext?.Items);
            var appCaches = new AppCaches(
                new DeepCloneAppCache(new ObjectCacheAppCache()),
                requestCache,
                new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

            builder.AddUmbracoCore(webHostEnvironment,
                Assembly.GetEntryAssembly(),
                appCaches,
                loggingConfig,
                builder.Config,
                UmbracoCoreServiceCollectionExtensions.ConfigureSomeMorebits);

            return builder;
        }

        public static IUmbracoBuilder AddHostedServices(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoHostedServices();
            return builder;
        }

        public static IUmbracoBuilder AddHttpClients(this IUmbracoBuilder builder)
        {
            builder.Services.AddUmbracoHttpClients();
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
    }
}
