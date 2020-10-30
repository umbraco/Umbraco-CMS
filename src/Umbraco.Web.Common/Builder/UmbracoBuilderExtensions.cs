using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Common.ModelBinders;

namespace Umbraco.Web.Common.Builder
{
    // TODO: We could add parameters to configure each of these for flexibility
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbraco(this IServiceCollection services, IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));
            if (webHostEnvironment is null) throw new ArgumentNullException(nameof(webHostEnvironment));
            if (config is null) throw new ArgumentNullException(nameof(config));

            services.AddLazySupport();

            var builder = new UmbracoBuilder(services, webHostEnvironment, config);
            return builder;
        }

        public static IUmbracoBuilder WithConfiguration(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithConfiguration), () => builder.Services.AddUmbracoConfiguration(builder.Config));

        public static IUmbracoBuilder WithCore(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithCore), () => builder.Services.AddUmbracoCore(builder.WebHostEnvironment, builder.Config));

        public static IUmbracoBuilder WithHostedServices(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithHostedServices), () => builder.Services.AddUmbracoHostedServices());

        public static IUmbracoBuilder WithMiniProfiler(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithMiniProfiler), () =>
            builder.Services.AddMiniProfiler(options =>
            {
                options.ShouldProfile = request => false; // WebProfiler determine and start profiling. We should not use the MiniProfilerMiddleware to also profile
            }));

        public static IUmbracoBuilder WithMvcAndRazor(this IUmbracoBuilder builder, Action<MvcOptions> mvcOptions = null, Action<IMvcBuilder> mvcBuilding = null)
            => builder.AddWith(nameof(WithMvcAndRazor), () =>
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
            });

        public static IUmbracoBuilder WithRuntimeMinifier(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithRuntimeMinifier), () => builder.Services.AddUmbracoRuntimeMinifier(builder.Config));

        public static IUmbracoBuilder WithWebComponents(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithWebComponents), () => builder.Services.AddUmbracoWebComponents());

        public static IUmbracoBuilder WithWebServer(this IUmbracoBuilder builder)
            => builder.AddWith(nameof(WithWebServer), () =>
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
            });
    }
}
