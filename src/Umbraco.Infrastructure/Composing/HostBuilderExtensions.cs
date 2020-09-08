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
        /// Used to assing a custom service provider factory to use Umbraco's container
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>Doesn't do much yet bu presumably will in the future.</remarks>
        public static IHostBuilder UseUmbraco(this IHostBuilder builder)
        {
            return builder.UseSerilog();
        }
    }
}
