using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Represents the application caches.
/// </summary>
public class AppCaches : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppCaches" /> with cache providers.
    /// </summary>
    public AppCaches(
        IAppPolicyCache runtimeCache,
        IRequestCache requestCache,
        IsolatedCaches isolatedCaches)
    {
        RuntimeCache = runtimeCache ?? throw new ArgumentNullException(nameof(runtimeCache));
        RequestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));
        IsolatedCaches = isolatedCaches ?? throw new ArgumentNullException(nameof(isolatedCaches));
    }

    /// <summary>
    ///     Gets the special disabled instance.
    /// </summary>
    /// <remarks>
    ///     <para>When used by repositories, all cache policies apply, but the underlying caches do not cache anything.</para>
    ///     <para>Used by tests.</para>
    /// </remarks>
    public static AppCaches Disabled { get; } = new(NoAppCache.Instance, NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));

    /// <summary>
    ///     Gets the special no-cache instance.
    /// </summary>
    /// <remarks>
    ///     <para>When used by repositories, all cache policies are bypassed.</para>
    ///     <para>Used by repositories that do no cache.</para>
    /// </remarks>
    public static AppCaches NoCache { get; } = new(NoAppCache.Instance, NoAppCache.Instance, new IsolatedCaches(_ => NoAppCache.Instance));

    /// <summary>
    ///     Gets the per-request cache.
    /// </summary>
    /// <remarks>
    ///     <para>The per-request caches works on top of the current HttpContext items.</para>
    ///     <para>Outside a web environment, the behavior of that cache is unspecified.</para>
    /// </remarks>
    public IRequestCache RequestCache { get; }

    /// <summary>
    ///     Gets the runtime cache.
    /// </summary>
    /// <remarks>
    ///     <para>The runtime cache is the main application cache.</para>
    /// </remarks>
    public IAppPolicyCache RuntimeCache { get; }

    /// <summary>
    ///     Gets the isolated caches.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Isolated caches are used by e.g. repositories, to ensure that each cached entity
    ///         type has its own cache, so that lookups are fast and the repository does not need to
    ///         search through all keys on a global scale.
    ///     </para>
    /// </remarks>
    public IsolatedCaches IsolatedCaches { get; }

    public static AppCaches Create(IRequestCache requestCache) =>
        new(
            new DeepCloneAppCache(new ObjectCacheAppCache()),
            requestCache,
            new IsolatedCaches(type => new DeepCloneAppCache(new ObjectCacheAppCache())));

    public void Dispose() =>

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                RuntimeCache.DisposeIfDisposable();
                RequestCache.DisposeIfDisposable();
                IsolatedCaches.Dispose();
            }

            _disposedValue = true;
        }
    }
}
