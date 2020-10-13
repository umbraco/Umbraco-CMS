using Microsoft.Extensions.Hosting;
using Serilog;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Extends the <see cref="IHostBuilder"/> to enable Umbraco to be used as the service container.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Assigns a custom service provider factory to use Umbraco's container
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHostBuilder UseUmbraco(this IHostBuilder builder)
        {
            return builder
                .UseUmbraco(new UmbracoServiceProviderFactory());
        }

        /// <summary>
        /// Assigns a custom service provider factory to use Umbraco's container
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="umbracoServiceProviderFactory"></param>
        /// <returns></returns>
        public static IHostBuilder UseUmbraco(this IHostBuilder builder, UmbracoServiceProviderFactory umbracoServiceProviderFactory)
            => builder.UseServiceProviderFactory(umbracoServiceProviderFactory);
    }
}
