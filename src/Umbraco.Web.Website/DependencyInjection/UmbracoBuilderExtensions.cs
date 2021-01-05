using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Infrastructure.DependencyInjection;
using Umbraco.Infrastructure.PublishedCache.DependencyInjection;
using Umbraco.Web.Website.Collections;
using Umbraco.Web.Website.Controllers;
using Umbraco.Web.Website.Routing;
using Umbraco.Web.Website.ViewEngines;

namespace Umbraco.Web.Website.DependencyInjection
{
    /// <summary>
    /// <see cref="IUmbracoBuilder"/> extensions for umbraco front-end website
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Add services for the umbraco front-end website
        /// </summary>
        public static IUmbracoBuilder AddWebsite(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<NoContentRoutes>();

            builder.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                 .Add(builder.TypeLoader.GetSurfaceControllers());

            // Configure MVC startup options for custom view locations
            builder.Services.AddTransient<IConfigureOptions<RazorViewEngineOptions>, RenderRazorViewEngineOptionsSetup>();
            builder.Services.AddTransient<IConfigureOptions<RazorViewEngineOptions>, PluginRazorViewEngineOptionsSetup>();

            // Wraps all existing view engines in a ProfilerViewEngine
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

            // TODO figure out if we need more to work on load balanced setups
            builder.Services.AddDataProtection();

            builder.Services.AddScoped<UmbracoRouteValueTransformer>();
            builder.Services.AddSingleton<IUmbracoRenderingDefaults, UmbracoRenderingDefaults>();

            builder.AddDistributedCache();

            return builder;
        }

    }
}
