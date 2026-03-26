// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

public class UmbracoWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly Func<IHostBuilder> _createHostBuilder;
    private IHost _host;

    /// <summary>
    ///     Gets or sets the content root path. When set, overrides the default content root
    ///     that WebApplicationFactory resolves from the TStartup assembly.
    /// </summary>
    /// <remarks>
    ///     This is applied in <see cref="ConfigureWebHost"/> which runs after the factory's
    ///     own SetContentRoot, ensuring our value takes precedence.
    /// </remarks>
    public string? ContentRoot { get; set; }

    /// <summary>
    ///     Constructor to create a new WebApplicationFactory.
    /// </summary>
    /// <param name="createHostBuilder">Method to create the IHostBuilder</param>
    public UmbracoWebApplicationFactory(Func<IHostBuilder> createHostBuilder) => _createHostBuilder = createHostBuilder;

    protected override IHostBuilder CreateHostBuilder() => _createHostBuilder();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (ContentRoot is not null)
        {
            builder.UseContentRoot(ContentRoot);
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _host = builder.Build();

        // Use Task.Run to avoid deadlocking with NUnit 4's SynchronizationContext.
        Task.Run(() => _host.StartAsync()).GetAwaiter().GetResult();

        return _host;
    }

    public void ClearHost() => Task.Run(() => _host.StopAsync()).GetAwaiter().GetResult();
}
