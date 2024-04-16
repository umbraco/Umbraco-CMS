using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Middleware;
using Umbraco.Cms.Api.Management.Routing;
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
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IUmbracoApplicationBuilderContext UseBackOffice(this IUmbracoApplicationBuilderContext builder)
    {
        builder.AppBuilder.UseMiddleware<BackOfficeExternalLoginProviderErrorMiddleware>();
        return builder;
    }

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

        BackOfficeAreaRoutes backOfficeRoutes = app.ApplicationServices.GetRequiredService<BackOfficeAreaRoutes>();
        backOfficeRoutes.CreateRoutes(app.EndpointRouteBuilder);

        app.UseUmbracoPreviewEndpoints();

        return app;
    }
}
