using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

public interface IWebApplicationFactoryAdapter : IDisposable, IAsyncDisposable
{
    IWebApplicationFactoryAdapter WithWebHostBuilder(Action<IWebHostBuilder> configuration);

    void UseKestrel();

    void UseKestrel(int port);

    void UseKestrel(Action<KestrelServerOptions> configureKestrelOptions);

    void StartServer();

    HttpClient CreateClient();

    HttpClient CreateClient(WebApplicationFactoryClientOptions options);

    HttpClient CreateDefaultClient(params DelegatingHandler[] handlers);

    HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers);

    TestServer Server { get; }

    IServiceProvider Services { get; }

    IReadOnlyList<IWebApplicationFactoryAdapter> Factories { get; }

    WebApplicationFactoryClientOptions ClientOptions { get; }
}

public class WebApplicationFactoryAdapter<TStartup> : IWebApplicationFactoryAdapter
    where TStartup : class
{
    private WebApplicationFactory<TStartup> actual;

    public WebApplicationFactoryAdapter(Func<IHostBuilder> hostBuilder, Action<IWebHostBuilder> webHostConfig)
        : this(new UmbracoWebApplicationFactory<TStartup>(hostBuilder).WithWebHostBuilder(webHostConfig))
    {
    }

    protected WebApplicationFactoryAdapter(WebApplicationFactory<TStartup> actual)
    {
        this.actual = actual;
    }

    public IWebApplicationFactoryAdapter WithWebHostBuilder(Action<IWebHostBuilder> configuration) => new WebApplicationFactoryAdapter<TStartup>(actual.WithWebHostBuilder(configuration));

    public void UseKestrel() => actual.UseKestrel();

    public void UseKestrel(int port) => actual.UseKestrel(port);

    public void UseKestrel(Action<KestrelServerOptions> configureKestrelOptions) => actual.UseKestrel(configureKestrelOptions);

    public void StartServer() => actual.StartServer();

    public HttpClient CreateClient() => actual.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => actual.CreateClient(options);

    public HttpClient CreateDefaultClient(params DelegatingHandler[] handlers) => actual.CreateDefaultClient(handlers);

    public HttpClient CreateDefaultClient(Uri baseAddress, params DelegatingHandler[] handlers) => actual.CreateDefaultClient(baseAddress, handlers);

    public void Dispose() => actual.Dispose();

    public ValueTask DisposeAsync() => actual.DisposeAsync();

    public TestServer Server => actual.Server;

    public IServiceProvider Services => actual.Services;

    public IReadOnlyList<IWebApplicationFactoryAdapter> Factories => actual.Factories.Select(x => new WebApplicationFactoryAdapter<TStartup>(x)).ToList().AsReadOnly();

    public WebApplicationFactoryClientOptions ClientOptions => actual.ClientOptions;
}
