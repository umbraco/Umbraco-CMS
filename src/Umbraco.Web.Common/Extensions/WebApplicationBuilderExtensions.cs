using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Configuration.Models;
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
        // Configure Umbraco defaults, but ignore decorated host builder and
        // don't add runtime as hosted service (this is replaced by the explicit BootUmbracoAsync)
        builder.Host.ConfigureUmbracoDefaults(false);

        // Do not enable static web assets on production environments,
        // because the files are already copied to the publish output folder.
        if (builder.Configuration.GetRuntimeMode() != RuntimeMode.Production)
        {
            builder.WebHost.UseStaticWebAssets();
        }

        return builder.Services.AddUmbraco(builder.Environment, builder.Configuration);
    }
}
