using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Extensions;

/// <summary>
///     <see cref="IUmbracoEndpointBuilderContext" /> extensions for Umbraco
/// </summary>
public static partial class UmbracoApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds all required middleware to run the back office
    /// </summary>
    /// <param name="builder">The Umbraco application builder context.</param>
    /// <returns>The builder for chaining.</returns>
    public static IUmbracoApplicationBuilderContext UseBackOffice(this IUmbracoApplicationBuilderContext builder)
    {
        builder.AppBuilder.UseMiddleware<BackOfficeExternalLoginProviderErrorMiddleware>();
        return builder;
    }

    /// <summary>
    /// Configures the endpoint routes required for the Umbraco back office, including administrative and preview endpoints.
    /// Should be called after routing, authentication, and authorization middleware have been configured.
    /// </summary>
    /// <param name="app">The <see cref="IUmbracoEndpointBuilderContext"/> to configure with back office endpoints.</param>
    /// <returns>The same <see cref="IUmbracoEndpointBuilderContext"/> instance, for chaining.</returns>
    public static IUmbracoEndpointBuilderContext UseBackOfficeEndpoints(this IUmbracoEndpointBuilderContext app)
    {
        // NOTE: This method will have been called after UseRouting, UseAuthentication, UseAuthorization
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (!app.RuntimeState.UmbracoCanBoot())
        {
            return app;
        }

        WarnIfWebsiteEndpointsAlreadyRegistered(app);

        BackOfficeAreaRoutes backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
        backOfficeRoutes.CreateRoutes(app.EndpointRouteBuilder);

        app.UseUmbracoPreviewEndpoints();

        return app;
    }

    private static void WarnIfWebsiteEndpointsAlreadyRegistered(IUmbracoEndpointBuilderContext app)
    {
        bool dynamicRouteAlreadyRegistered = app.EndpointRouteBuilder.DataSources
            .SelectMany(ds => ds.Endpoints)
            .OfType<RouteEndpoint>()
            .Any(e => string.Equals(
                e.RoutePattern.RawText,
                Constants.Web.Routing.DynamicRoutePattern,
                StringComparison.Ordinal));

        if (dynamicRouteAlreadyRegistered is false)
        {
            return;
        }

#pragma warning disable CS0436 // Type conflicts with imported type
        ILogger logger = app.ApplicationServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(UmbracoApplicationBuilderExtensions).FullName!);
#pragma warning restore CS0436 // Type conflicts with imported type

        logger.LogWarning(
            "UseWebsiteEndpoints() appears to have been called before UseBackOfficeEndpoints() in the WithEndpoints configuration. " +
            "The required order is UseBackOfficeEndpoints() first, then UseWebsiteEndpoints(). " +
            "With the order reversed the front-end dynamic content route shadows the backoffice routes, and Umbraco will appear to start with blank pages, broken static assets, and a non-functional install screen with no further errors. " +
            "Update your Program.cs WithEndpoints callback to call UseBackOfficeEndpoints() first.");
    }
}
