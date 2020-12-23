using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.DependencyInjection;
using Umbraco.Infrastructure.PublishedCache.Extensions;
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
            // Set the render & plugin view engines (Super complicated, but this allows us to use the IServiceCollection
            // to inject dependencies into the viewEngines)
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, RenderMvcViewOptionsSetup>();
            builder.Services.AddSingleton<IRenderViewEngine, RenderViewEngine>();
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, PluginMvcViewOptionsSetup>();
            builder.Services.AddSingleton<IPluginViewEngine, PluginViewEngine>();

            // Wraps all existing view engines in a ProfilerViewEngine
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

            // TODO figure out if we need more to work on load balanced setups
            builder.Services.AddDataProtection();

            builder.Services.AddScoped<UmbracoRouteValueTransformer>();
            builder.Services.AddSingleton<IUmbracoRenderingDefaults, UmbracoRenderingDefaults>();

            builder.AddNuCache();

            return builder;
        }

    }
}
