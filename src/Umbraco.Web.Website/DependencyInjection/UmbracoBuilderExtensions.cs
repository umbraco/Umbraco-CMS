using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Collections;
using Umbraco.Cms.Web.Website.Models;
using Umbraco.Cms.Web.Website.Routing;
using Umbraco.Cms.Web.Website.ViewEngines;
using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Umbraco.Extensions;

/// <summary>
///     <see cref="IUmbracoBuilder" /> extensions for umbraco front-end website
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Add services for the umbraco front-end website
    /// </summary>
    public static IUmbracoBuilder AddWebsite(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<SurfaceControllerTypeCollectionBuilder>()
            .Add(builder.TypeLoader.GetSurfaceControllers());

        // Configure MVC startup options for custom view locations
        builder.Services.ConfigureOptions<RenderRazorViewEngineOptionsSetup>();
        builder.Services.ConfigureOptions<PluginRazorViewEngineOptionsSetup>();

        // Wraps all existing view engines in a ProfilerViewEngine
        builder.Services
            .AddTransient<IConfigureOptions<MvcViewOptions>, ProfilingViewEngineWrapperMvcViewOptionsSetup>();

        // TODO figure out if we need more to work on load balanced setups
        builder.Services.AddDataProtection();
        builder.Services.AddAntiforgery();

        builder.Services.AddSingleton<UmbracoRouteValueTransformer>();
        builder.Services.AddSingleton<IControllerActionSearcher, ControllerActionSearcher>();
        builder.Services.TryAddEnumerable(Singleton<MatcherPolicy, NotFoundSelectorPolicy>());
        builder.Services.AddSingleton<IUmbracoRouteValuesFactory, UmbracoRouteValuesFactory>();
        builder.Services.AddSingleton<IRoutableDocumentFilter, RoutableDocumentFilter>();

        builder.Services.AddSingleton<FrontEndRoutes>();

        builder.Services.AddSingleton<MemberModelBuilderFactory>();

        builder.Services.AddSingleton<IPublicAccessRequestHandler, PublicAccessRequestHandler>();
        builder.Services.AddSingleton<BasicAuthenticationMiddleware>();

        builder
            .AddDistributedCache()
            .AddModelsBuilder();

        builder.AddMembersIdentity();

        return builder;
    }
}
