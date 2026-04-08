using Microsoft.AspNetCore.Mvc.Testing;

namespace Umbraco.Cms.Tests.Integration.TestServerTest.Fixtures;

/// <summary>
///     Non-generic adapter interface over <see cref="WebApplicationFactory{TEntryPoint}"/>.
///     This abstraction eliminates the generic type parameter coupling and the deps.json file requirement
///     for downstream consumers.
/// </summary>
public interface IWebApplicationFactoryAdapter : IAsyncDisposable, IDisposable
{
    /// <summary>
    ///     Gets the <see cref="IServiceProvider"/> from the test server.
    /// </summary>
    IServiceProvider Services { get; }

    /// <summary>
    ///     Creates an <see cref="HttpClient"/> for making requests to the test server.
    /// </summary>
    HttpClient CreateClient(WebApplicationFactoryClientOptions options);
}
