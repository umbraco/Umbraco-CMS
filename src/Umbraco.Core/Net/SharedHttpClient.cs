namespace Umbraco.Cms.Core.Net;

/// <summary>
/// Provides a correctly configured, process-wide <see cref="HttpClient"/> for Umbraco's own outbound
/// requests from code that cannot take an <c>IHttpClientFactory</c> dependency.
/// </summary>
/// <remarks>
/// <para>
/// <b>New code should not use this.</b> Inject <c>IHttpClientFactory</c> and resolve a named client
/// registered in <c>AddHttpClients()</c> instead. That is the supported approach: it gives consumers a
/// configuration point, and it keeps the calling code testable.
/// </para>
/// <para>
/// This type exists for the call sites that predate that guidance and cannot adopt it without a breaking
/// change - a public abstract base class whose constructor signature is part of the contract for
/// third-party implementations, or a type instantiated outside the DI container. It is a narrowly scoped
/// shim, not a general-purpose alternative.
/// </para>
/// <para>
/// The instance is created through a static initializer so that all configuration - in particular the
/// default request headers - completes before any thread can observe it. Mutating
/// <see cref="HttpClient.DefaultRequestHeaders"/> once a request is in flight is not thread safe and can
/// fault the request writer (#23697).
/// </para>
/// <para>
/// Because the instance is shared, callers must treat it as immutable: do not assign
/// <see cref="HttpClient.Timeout"/>, <see cref="HttpClient.BaseAddress"/> or default headers on it. Apply
/// a per-request timeout with a linked <see cref="CancellationTokenSource"/> instead.
/// </para>
/// </remarks>
internal static class SharedHttpClient
{
    private static readonly TimeSpan _connectionLifetime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets the shared <see cref="HttpClient"/> instance.
    /// </summary>
    internal static HttpClient Instance { get; } = Create();

    private static HttpClient Create()
    {
        // A pooled connection lifetime is required because this client lives for the lifetime of the
        // process; without it, DNS changes are never picked up. This mirrors what IHttpClientFactory does
        // by rotating its handlers.
        var handler = new SocketsHttpHandler { PooledConnectionLifetime = _connectionLifetime };

        var client = new HttpClient(handler);
        client.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.HttpClients.Headers.UserAgentProductName);

        return client;
    }
}
