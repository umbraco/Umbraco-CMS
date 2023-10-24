using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.Routing;
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
        KeepAliveSettings keepAliveSettings =
            builder.ApplicationServices.GetRequiredService<IOptions<KeepAliveSettings>>().Value;
        IHostingEnvironment hostingEnvironment = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
        builder.AppBuilder.Map(
            hostingEnvironment.ToAbsolute(keepAliveSettings.KeepAlivePingUrl),
            a => a.UseMiddleware<KeepAliveMiddleware>());

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

        app.UseUmbracoRuntimeMinificationEndpoints();
        app.UseUmbracoPreviewEndpoints();

        return app;
    }

    /// <summary>
    ///     Adds CORS middleware to the pipeline for the BackOffice.
    ///     This is useful if you are running the BackOffice on a different port than the backend server.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="origins">
    ///     A list of allowed fully-qualified hostnames that should be whitelisted.
    ///     If this is omitted, all origins will be allowed, which will not work with credentials.
    /// </param>
    /// <returns></returns>
    public static IApplicationBuilder UseBackOfficeCors(
        this IApplicationBuilder app, params string[] origins)
    {
        app.UseCors(u =>
        {
            u.AllowCredentials();
            u.AllowAnyMethod();
            u.AllowAnyHeader();
            u.WithExposedHeaders("Location");
            if (origins.Length > 0)
            {
                u.WithOrigins(origins);
            }
            else
            {
                u.AllowAnyOrigin();
            }
        });
        return app;
    }
}
