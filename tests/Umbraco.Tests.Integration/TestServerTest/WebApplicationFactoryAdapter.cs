using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

/// <summary>
/// A non-generic view of a <see cref="WebApplicationFactory{TEntryPoint}"/>, so a test base class can expose
/// the factory without fixing its entry point type.
/// </summary>
/// <remarks>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> requires the <c>.deps.json</c> file of the entry point's
/// assembly to be present in the test output. Hiding the type argument lets the factory be created for the
/// concrete test fixture, whose assembly always has that file, rather than for a type in a referenced package.
/// </remarks>
public interface IWebApplicationFactoryAdapter : IDisposable, IAsyncDisposable
{
    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.Server"/>
    TestServer Server { get; }

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.Services"/>
    IServiceProvider Services { get; }

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.Factories"/>
    IReadOnlyList<IWebApplicationFactoryAdapter> Factories { get; }

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.ClientOptions"/>
    WebApplicationFactoryClientOptions ClientOptions { get; }

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.WithWebHostBuilder(Action{IWebHostBuilder})"/>
    IWebApplicationFactoryAdapter WithWebHostBuilder(Action<IWebHostBuilder> configuration);

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.UseKestrel()"/>
    void UseKestrel();

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.UseKestrel(int)"/>
    void UseKestrel(int port);

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.UseKestrel(Action{KestrelServerOptions})"/>
    void UseKestrel(Action<KestrelServerOptions> configureKestrelOptions);

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.StartServer()"/>
    void StartServer();

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.CreateClient()"/>
    HttpClient CreateClient();

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.CreateClient(WebApplicationFactoryClientOptions)"/>
    HttpClient CreateClient(WebApplicationFactoryClientOptions options);

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.CreateDefaultClient(DelegatingHandler[])"/>
    HttpClient CreateDefaultClient(params DelegatingHandler[] handlers);

    /// <inheritdoc cref="WebApplicationFactory{TEntryPoint}.CreateDefaultClient(Uri, DelegatingHandler[])"/>
    HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers);
}

/// <summary>
/// Wraps a <see cref="WebApplicationFactory{TEntryPoint}"/> as an <see cref="IWebApplicationFactoryAdapter"/>.
/// </summary>
/// <typeparam name="TEntryPoint">The type whose assembly's <c>.deps.json</c> file the factory resolves.</typeparam>
public class WebApplicationFactoryAdapter<TEntryPoint> : IWebApplicationFactoryAdapter
    where TEntryPoint : class
{
    private readonly WebApplicationFactory<TEntryPoint> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationFactoryAdapter{TEntryPoint}"/> class.
    /// </summary>
    /// <param name="createHostBuilder">Creates the host builder for the application under test.</param>
    /// <param name="configureWebHost">Configures the web host, for example to replace services for testing.</param>
    public WebApplicationFactoryAdapter(Func<IHostBuilder> createHostBuilder, Action<IWebHostBuilder> configureWebHost)
        : this(new UmbracoWebApplicationFactory<TEntryPoint>(createHostBuilder).WithWebHostBuilder(configureWebHost))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationFactoryAdapter{TEntryPoint}"/> class.
    /// </summary>
    /// <param name="factory">The factory to wrap.</param>
    protected WebApplicationFactoryAdapter(WebApplicationFactory<TEntryPoint> factory) => _factory = factory;

    /// <inheritdoc />
    public TestServer Server => _factory.Server;

    /// <inheritdoc />
    public IServiceProvider Services => _factory.Services;

    /// <inheritdoc />
    public IReadOnlyList<IWebApplicationFactoryAdapter> Factories
        => _factory.Factories.Select(x => new WebApplicationFactoryAdapter<TEntryPoint>(x)).ToList().AsReadOnly();

    /// <inheritdoc />
    public WebApplicationFactoryClientOptions ClientOptions => _factory.ClientOptions;

    /// <inheritdoc />
    public IWebApplicationFactoryAdapter WithWebHostBuilder(Action<IWebHostBuilder> configuration)
        => new WebApplicationFactoryAdapter<TEntryPoint>(_factory.WithWebHostBuilder(configuration));

    /// <inheritdoc />
    public void UseKestrel() => _factory.UseKestrel();

    /// <inheritdoc />
    public void UseKestrel(int port) => _factory.UseKestrel(port);

    /// <inheritdoc />
    public void UseKestrel(Action<KestrelServerOptions> configureKestrelOptions) => _factory.UseKestrel(configureKestrelOptions);

    /// <inheritdoc />
    public void StartServer() => _factory.StartServer();

    /// <inheritdoc />
    public HttpClient CreateClient() => _factory.CreateClient();

    /// <inheritdoc />
    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _factory.CreateClient(options);

    /// <inheritdoc />
    public HttpClient CreateDefaultClient(params DelegatingHandler[] handlers) => _factory.CreateDefaultClient(handlers);

    /// <inheritdoc />
    public HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers)
        => _factory.CreateDefaultClient(baseAddress, handlers);

    /// <inheritdoc />
    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
