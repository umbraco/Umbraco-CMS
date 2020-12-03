using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Web.Website.ViewEngines;

namespace Umbraco.Extensions
{
    public static class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddUmbracoWebsite(this IUmbracoBuilder builder)
        {
            // Set the render & plugin view engines (Super complicated, but this allows us to use the IServiceCollection
            // to inject dependencies into the viewEngines)
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, RenderMvcViewOptionsSetup>();
            builder.Services.AddSingleton<IRenderViewEngine, RenderViewEngine>();
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, PluginMvcViewOptionsSetup>();
            builder.Services.AddSingleton<IPluginViewEngine, PluginViewEngine>();

            // Wraps all existing view engines in a ProfilerViewEngine
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

            //TODO figure out if we need more to work on load balanced setups
            builder.Services.AddDataProtection();

            return builder;
        }


    }
}
