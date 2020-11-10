using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Web.Website.ViewEngines;

namespace Umbraco.Extensions
{
    public static class UmbracoWebstiteServiceCollectionExtensions
    {
        public static void AddUmbracoWebsite(this IServiceCollection services)
        {
            services.AddSingleton<IControllerActivator, ServiceBasedControllerActivator>();

            // Set the render & plugin view engines (Super complicated, but this allows us to use the IServiceCollection
            // to inject dependencies into the viewEngines)
            services.AddTransient<IConfigureOptions<MvcViewOptions>, RenderMvcViewOptionsSetup>();
            services.AddSingleton<IRenderViewEngine, RenderViewEngine>();
            services.AddTransient<IConfigureOptions<MvcViewOptions>, PluginMvcViewOptionsSetup>();
            services.AddSingleton<IPluginViewEngine, PluginViewEngine>();

            // Wraps all existing view engines in a ProfilerViewEngine
            services.AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

            //TODO figure out if we need more to work on load balanced setups
            services.AddDataProtection();
        }

    }
}
