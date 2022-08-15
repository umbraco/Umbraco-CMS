// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

public class UmbracoWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly Action<IHost> _beforeStart;
    private readonly Func<IHostBuilder> _createHostBuilder;

    /// <summary>
    ///     Constructor to create a new WebApplicationFactory
    /// </summary>
    /// <param name="createHostBuilder">Method to create the IHostBuilder</param>
    /// <param name="beforeStart">Method to perform an action before IHost starts</param>
    public UmbracoWebApplicationFactory(Func<IHostBuilder> createHostBuilder) => _createHostBuilder = createHostBuilder;

    protected override IHostBuilder CreateHostBuilder() => _createHostBuilder();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();

        _beforeStart?.Invoke(host);

        host.Start();

        return host;
    }
}
