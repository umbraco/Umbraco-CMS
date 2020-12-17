using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Extends the <see cref="IHostBuilder"/> to add CoreRuntime as a HostedService
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Adds CoreRuntime as HostedService
        /// </summary>
        /// <remarks>
        /// Should be called before ConfigureWebDefaults.
        /// </remarks>
        public static IHostBuilder UseUmbraco(this IHostBuilder builder)
        {
            _ = builder.ConfigureServices((context, services) =>
                  services.AddSingleton<IHostedService>(factory => factory.GetRequiredService<IRuntime>()));

            return builder;
        }
    }
}
