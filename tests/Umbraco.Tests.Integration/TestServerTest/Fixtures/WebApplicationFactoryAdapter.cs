using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest.Fixtures;

/// <summary>
///     Wraps <see cref="UmbracoWebApplicationFactory{TStartup}"/> to implement <see cref="IWebApplicationFactoryAdapter"/>.
///     The generic type parameter is hidden behind the non-generic interface.
/// </summary>
public class WebApplicationFactoryAdapter<TStartup> : IWebApplicationFactoryAdapter
    where TStartup : class
{
    private readonly UmbracoWebApplicationFactory<TStartup> _inner;

    public WebApplicationFactoryAdapter(
        Func<IHostBuilder> createHostBuilder,
        Action<IWebHostBuilder> configureWebHost)
    {
        _inner = new UmbracoWebApplicationFactory<TStartup>(createHostBuilder);

        // Apply additional web host configuration via WithWebHostBuilder
        // We need to trigger the factory to build by accessing a property or creating a client
        _configureWebHost = configureWebHost;
    }

    private readonly Action<IWebHostBuilder> _configureWebHost;
    private WebApplicationFactory<TStartup> _configured;

    private WebApplicationFactory<TStartup> ConfiguredFactory
    {
        get
        {
            if (_configured == null)
            {
                _configured = _inner.WithWebHostBuilder(builder =>
                {
                    _configureWebHost?.Invoke(builder);
                });
            }

            return _configured;
        }
    }

    public IServiceProvider Services => ConfiguredFactory.Services;

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) =>
        ConfiguredFactory.CreateClient(options);

    public void Dispose()
    {
        _configured?.Dispose();
        _inner.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_configured != null)
        {
            await _configured.DisposeAsync();
        }

        await _inner.DisposeAsync();
    }

    /// <summary>
    ///     Creates an <see cref="IWebApplicationFactoryAdapter"/> using reflection to instantiate with the consumer's
    ///     test class type. This ensures the correct assembly's deps.json file is used.
    /// </summary>
    /// <param name="consumerType">The type of the test class (used for assembly resolution).</param>
    /// <param name="createHostBuilder">Factory method for creating the host builder.</param>
    /// <param name="configureWebHost">Action to configure the web host builder.</param>
    /// <returns>An <see cref="IWebApplicationFactoryAdapter"/> instance.</returns>
    public static IWebApplicationFactoryAdapter Create(
        Type consumerType,
        Func<IHostBuilder> createHostBuilder,
        Action<IWebHostBuilder> configureWebHost)
    {
        var adapterType = typeof(WebApplicationFactoryAdapter<>).MakeGenericType(consumerType);
        var ctor = adapterType.GetConstructor([typeof(Func<IHostBuilder>), typeof(Action<IWebHostBuilder>)])
            ?? throw new InvalidOperationException($"Could not find constructor on {adapterType.FullName}");

        return (IWebApplicationFactoryAdapter)ctor.Invoke([createHostBuilder, configureWebHost]);
    }
}
