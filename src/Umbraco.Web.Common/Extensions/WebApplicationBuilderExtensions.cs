using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Extensions;

public static class WebApplicationBuilderExtensions{

    /// <summary>
    /// Builds the <see cref="WebApplication"/> and ensure Umbraco is ready for middleware to be added.
    /// </summary>
    /// <returns>A configured <see cref="WebApplication"/>.</returns>
    public static async Task<WebApplication> BuildForUmbracoAsync(this WebApplicationBuilder builder)
    {
        // Builds the default web application
        WebApplication app = builder.Build();

        // Ensure the Umbraco runtime is started before middleware is added and stopped doing shutdown
        IRuntime umbracoRuntime = app.Services.GetRequiredService<IRuntime>();
        IHostApplicationLifetime applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        CancellationTokenRegistration cancellationTokenRegistration = applicationLifetime.ApplicationStopping.Register((o, token) => umbracoRuntime.StopAsync(token), null);

        await umbracoRuntime.StartAsync(cancellationTokenRegistration.Token);

        return app;
    }
}
