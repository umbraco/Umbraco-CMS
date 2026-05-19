using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

/// <summary>
///     Umbraco specific extensions for the <see cref="IHostBuilder" /> interface.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    ///     Configures an existing <see cref="IHostBuilder" /> with defaults for an Umbraco application.
    /// </summary>
    public static IHostBuilder ConfigureUmbracoDefaults(this IHostBuilder builder)
        => builder.ConfigureUmbracoDefaults(true);

    internal static IHostBuilder ConfigureUmbracoDefaults(this IHostBuilder builder, bool addRuntimeHostedService)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Register appsettings.Local.json as the last (highest-priority) JSON source when Umbraco is in
            // debug mode — gated on the same Umbraco:CMS:Hosting:Debug setting that backs
            // IHostingEnvironment.IsDebugMode. The template ships this set to true in appsettings.Development.json
            // and unset (false) in appsettings.json, so the file is loaded for developer setups and ignored in
            // production. The installer writes the connection string to this file (created on first save if absent)
            // so it stays local — appsettings.Local.json is gitignored and excluded from publish output in the
            // template's csproj.
            if (context.Configuration.GetValue<bool>("Umbraco:CMS:Hosting:Debug"))
            {
                config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
            }
        });

        builder.ConfigureLogging(x => x.ClearProviders());

        if (addRuntimeHostedService)
        {
            // Add the Umbraco IRuntime as hosted service
            builder.ConfigureServices(services => services.AddHostedService(factory => factory.GetRequiredService<IRuntime>()));
        }

        return new UmbracoHostBuilderDecorator(builder, OnHostBuilt);
    }

    // Runs before any IHostedService starts (including generic web host)
    private static void OnHostBuilt(IHost host) =>
        StaticServiceProvider.Instance = host.Services;
}
