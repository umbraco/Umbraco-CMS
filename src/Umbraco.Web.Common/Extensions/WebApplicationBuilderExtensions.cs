using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder" />.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Creates an <see cref="IUmbracoBuilder" /> and registers basic Umbraco services.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>
    /// The Umbraco builder.
    /// </returns>
    public static IUmbracoBuilder CreateUmbracoBuilder(this WebApplicationBuilder builder)
    {
        builder.Host.ConfigureUmbracoDefaults();
        builder.WebHost.UseStaticWebAssets();

        return builder.Services.AddUmbraco(builder.Environment, builder.Configuration);
    }
}
