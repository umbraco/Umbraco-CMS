// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

public class UmbracoWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly Func<IHostBuilder> _createHostBuilder;
    private IHost _host;

    /// <summary>
    ///     Constructor to create a new WebApplicationFactory
    /// </summary>
    /// <param name="createHostBuilder">Method to create the IHostBuilder</param>
    public UmbracoWebApplicationFactory(Func<IHostBuilder> createHostBuilder) => _createHostBuilder = createHostBuilder;

    protected override IHostBuilder CreateHostBuilder() => _createHostBuilder();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _host = builder.Build();
        _host.Start();

        return _host;
    }

    public void ClearHost()
    {
        _host.StopAsync().GetAwaiter().GetResult();
    }
}
