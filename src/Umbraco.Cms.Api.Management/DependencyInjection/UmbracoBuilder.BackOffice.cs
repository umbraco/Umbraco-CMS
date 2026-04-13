using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Web.Common.Hosting;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all required components to run the Umbraco back office.
    /// </summary>
    /// <remarks>
    /// This method calls <c>AddCore()</c> and <see cref="AddBackOfficeSignIn"/> internally
    /// to register core services, identity, and cookie authentication, then adds backoffice-specific
    /// services on top (OpenIddict, backoffice SPA infrastructure, token management).
    /// <para>
    /// For frontend-only deployments that only need basic authentication with backoffice credentials
    /// (no backoffice UI), use <see cref="AddBackOfficeSignIn"/> instead.
    /// </para>
    /// </remarks>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="configureMvc">Optional action to configure the MVC builder.</param>
    /// <returns>The Umbraco builder.</returns>
    public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder, Action<IMvcBuilder>? configureMvc = null) =>
        builder
            .AddCore(configureMvc)                   // All core services
            .AddBackOfficeSignIn()                   // Identity + Cookie authentication
            .AddBackOfficeCore()                     // IBackOfficePathGenerator, IBackOfficeEnabledMarker
            .AddBackOfficeOpenIddictServices()       // OpenIddict, application manager, middleware
            .AddTokenRevocation()                    // Token cleanup handlers
            .AddMembersIdentity();                   // Member identity (also needed for backoffice admin)

    /// <summary>
    /// Adds backoffice identity and cookie authentication without the full backoffice UI or OpenIddict.
    /// Use this for frontend-only deployments that need basic authentication with backoffice credentials.
    /// </summary>
    /// <remarks>
    /// This registers the backoffice identity system (user manager, sign-in manager) and cookie authentication
    /// schemes, but does NOT register OpenIddict, the Management API, or the backoffice SPA. It enables
    /// <c>BasicAuthenticationMiddleware</c> to authenticate users via a standalone server-rendered login page.
    /// <para>
    /// Requires <c>AddCore()</c> to have been called first.
    /// For full backoffice support, use <see cref="AddBackOffice"/> instead.
    /// </para>
    /// </remarks>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to configure.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> instance.</returns>
    public static IUmbracoBuilder AddBackOfficeSignIn(this IUmbracoBuilder builder) =>
        builder
            .AddBackOfficeIdentity()
            .AddBackOfficeCookieAuthentication();

    /// <summary>
    /// Registers the essential services required for the Umbraco back office, including the back office path generator and the physical file system implementation.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to add services to.</param>
    /// <returns>The updated <see cref="IUmbracoBuilder"/> instance.</returns>
    public static IUmbracoBuilder AddBackOfficeCore(this IUmbracoBuilder builder)
    {
        // Register marker indicating backoffice is enabled.
        builder.Services.AddSingleton<IBackOfficeEnabledMarker, BackOfficeEnabledMarker>();

        builder.Services.AddUnique<IBackOfficePathGenerator, UmbracoBackOfficePathGenerator>();
        builder.Services.AddUnique<IPhysicalFileSystem>(factory =>
        {
            var path = "~/";
            IHostingEnvironment hostingEnvironment = factory.GetRequiredService<IHostingEnvironment>();
            return new PhysicalFileSystem(
                factory.GetRequiredService<IIOHelper>(),
                hostingEnvironment,
                factory.GetRequiredService<ILogger<PhysicalFileSystem>>(),
                hostingEnvironment.MapPathContentRoot(path),
                hostingEnvironment.ToAbsolute(path));
        });

        return builder;
    }
}
