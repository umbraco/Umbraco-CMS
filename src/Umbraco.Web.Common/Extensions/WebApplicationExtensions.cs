using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplication" />.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Starts the <see cref="IRuntime" /> to ensure Umbraco is ready for middleware to be added.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns>
    /// A <see cref="Task" /> representing the asynchronous operation.
    /// </returns>
    public static async Task BootUmbracoAsync(this WebApplication app)
    {
        // Set static IServiceProvider before booting
        StaticServiceProvider.Instance = app.Services;

        // Ensure the Umbraco runtime is started before middleware is added and stopped when performing a graceful shutdown
        IRuntime umbracoRuntime = app.Services.GetRequiredService<IRuntime>();
        CancellationTokenRegistration cancellationTokenRegistration = app.Lifetime.ApplicationStopping.Register((_, token) => umbracoRuntime.StopAsync(token), null);

        await umbracoRuntime.StartAsync(cancellationTokenRegistration.Token);
    }
}
