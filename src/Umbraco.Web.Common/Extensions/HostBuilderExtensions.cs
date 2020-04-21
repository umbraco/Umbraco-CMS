using Microsoft.Extensions.Hosting;
using Serilog;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Extensions
{
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
                .UseSerilog()
                .UseUmbraco(new UmbracoServiceProviderFactory());
        }
    }
}
