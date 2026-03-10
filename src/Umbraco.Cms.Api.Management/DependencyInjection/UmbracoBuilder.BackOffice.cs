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
    /// This method calls <c>AddCore()</c> internally to register all core services,
    /// then adds backoffice-specific services on top.
    /// </remarks>
    /// <param name="builder">The Umbraco builder.</param>
    /// <param name="configureMvc">Optional action to configure the MVC builder.</param>
    /// <returns>The Umbraco builder.</returns>
    public static IUmbracoBuilder AddBackOffice(this IUmbracoBuilder builder, Action<IMvcBuilder>? configureMvc = null) =>
        builder
            .AddCore(configureMvc)           // All core services
            .AddBackOfficeCore()             // Backoffice-specific: IBackOfficePathGenerator
            .AddBackOfficeIdentity()         // Backoffice user identity
            .AddBackOfficeAuthentication()   // OpenIddict, authorization policies
            .AddTokenRevocation()            // Token cleanup handlers
            .AddMembersIdentity();           // Member identity (also needed for backoffice admin)

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
