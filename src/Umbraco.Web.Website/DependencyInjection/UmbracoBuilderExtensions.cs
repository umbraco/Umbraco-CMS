using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache.PartialViewCacheInvalidators;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Web.Common.Middleware;
using Umbraco.Cms.Web.Common.Routing;
using Umbraco.Cms.Web.Website.Cache.PartialViewCacheInvalidators;
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
    /// Add services for the umbraco front-end website.
    /// </summary>
    /// <remarks>
    /// This method assumes that either <c>AddBackOffice()</c> or <c>AddCore()</c> has already been called.
    /// It registers website-specific services such as Surface controllers, view engines, and routing.
    /// </remarks>
    /// <param name="builder">The Umbraco builder.</param>
    /// <returns>The Umbraco builder.</returns>
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

        builder.Services.AddSingleton<UmbracoRouteValueTransformer>(x => new UmbracoRouteValueTransformer(
            x.GetRequiredService<ILogger<UmbracoRouteValueTransformer>>(),
            x.GetRequiredService<IUmbracoContextAccessor>(),
            x.GetRequiredService<IPublishedRouter>(),
            x.GetRequiredService<IRuntimeState>(),
            x.GetRequiredService<IUmbracoRouteValuesFactory>(),
            x.GetRequiredService<IRoutableDocumentFilter>(),
            x.GetRequiredService<IDataProtectionProvider>(),
            x.GetRequiredService<IControllerActionSearcher>(),
            x.GetRequiredService<IPublicAccessRequestHandler>(),
            x.GetRequiredService<IUmbracoVirtualPageRoute>(),
            x.GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
            x.GetRequiredService<IDocumentUrlService>()));
        builder.Services.AddSingleton<IControllerActionSearcher, ControllerActionSearcher>();
        builder.Services.TryAddEnumerable(Singleton<MatcherPolicy, NotFoundSelectorPolicy>());
        builder.Services.AddSingleton<IUmbracoVirtualPageRoute, UmbracoVirtualPageRoute>();
        builder.Services.AddSingleton<IUmbracoRouteValuesFactory, UmbracoRouteValuesFactory>();
        builder.Services.AddSingleton<IRoutableDocumentFilter, RoutableDocumentFilter>();
        builder.Services.AddSingleton<MatcherPolicy, EagerMatcherPolicy>();
        builder.Services.AddSingleton<MatcherPolicy, SurfaceControllerMatcherPolicy>();

        builder.Services.AddSingleton<FrontEndRoutes>();

        builder.Services.AddSingleton<MemberModelBuilderFactory>();

        builder.Services.AddSingleton<IPublicAccessRequestHandler, PublicAccessRequestHandler>();
        builder.Services.AddSingleton<BasicAuthenticationMiddleware>();

        // Partial view cache invalidators
        builder.Services.AddUnique<IMemberPartialViewCacheInvalidator, MemberPartialViewCacheInvalidator>();

        builder.AddModelsBuilder();

        // Member identity for public member login
        builder.AddMembersIdentity();

        return builder;
    }
}
