using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Collections;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Middleware;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Cms.Web.Website.ViewEngines;

namespace Umbraco.Extensions
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
            builder.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
                 .Add(builder.TypeLoader.GetSurfaceControllers());

            // Configure MVC startup options for custom view locations
            builder.Services.ConfigureOptions<RenderRazorViewEngineOptionsSetup>();
            builder.Services.ConfigureOptions<PluginRazorViewEngineOptionsSetup>();

            // Wraps all existing view engines in a ProfilerViewEngine
            builder.Services.AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

            // TODO figure out if we need more to work on load balanced setups
            builder.Services.AddDataProtection();
            builder.Services.AddAntiforgery();

            builder.Services.AddScoped<UmbracoRouteValueTransformer>();
            builder.Services.AddSingleton<IControllerActionSearcher, ControllerActionSearcher>();
            builder.Services.AddSingleton<IUmbracoRouteValuesFactory, UmbracoRouteValuesFactory>();
            builder.Services.AddSingleton<IUmbracoRenderingDefaults, UmbracoRenderingDefaults>();
            builder.Services.AddSingleton<IRoutableDocumentFilter, RoutableDocumentFilter>();

            builder.Services.AddSingleton<FrontEndRoutes>();

            builder.Services.AddSingleton<MemberModelBuilderFactory>();

            builder.Services
                .AddSingleton<PublicAccessMiddleware>()
                .ConfigureOptions<PublicAccessMiddleware>();

            builder
                .AddDistributedCache()
                .AddModelsBuilder();

            return builder;
        }
    }
}
