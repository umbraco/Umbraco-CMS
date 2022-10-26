using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
///     Umbraco specific extensions for the <see cref="WebApplicationBuilder" />.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     Configures an existing <see cref="WebApplicationBuilder" /> with defaults for an Umbraco application.
    /// </summary>
    /// <remarks>
    ///     To be used with the new ASP.NET Core 6.0 minimal hosting model.
    /// </remarks>
    public static WebApplicationBuilder ConfigureUmbracoDefaults(this WebApplicationBuilder builder)
    {
#if DEBUG
        builder.Configuration.AddJsonFile(
                "appsettings.Local.json",
                true,
                true);

#endif
        builder.Logging.ClearProviders();

        return builder;
    }
}
