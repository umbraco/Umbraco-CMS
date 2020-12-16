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
        /// <para>When running the site should be called before ConfigureWebDefaults.</para>
        ///
        /// <para>
        /// When testing should be called after ConfigureWebDefaults to ensure UseTestDatabase is called before CoreRuntime
        /// starts or we initialize components with incorrect run level.
        /// </para>
        /// </remarks>
        public static IHostBuilder UseUmbraco(this IHostBuilder builder)
        {
            _ = builder.ConfigureServices((context, services) =>
                  services.AddSingleton<IHostedService>(factory => factory.GetRequiredService<IRuntime>()));

            return builder;
        }
    }
}
