using System;
using System.Web;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Represents the application-wide caches.
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// Initializes a new instance for use in the web
        /// </summary>
        public CacheHelper()
            : this(
                new HttpRuntimeCacheProvider(HttpRuntime.Cache),
                new StaticCacheProvider(),
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()))
        {
        }

        /// <summary>
        /// Initializes a new instance for use in the web
        /// </summary>
        public CacheHelper(System.Web.Caching.Cache cache)
            : this(
                new HttpRuntimeCacheProvider(cache),
                new StaticCacheProvider(),
                new HttpRequestCacheProvider(),
                new IsolatedRuntimeCache(t => new ObjectCacheRuntimeCacheProvider()))
        {
        }

        /// <summary>
        /// Initializes a new instance based on the provided providers
        /// </summary>
        public CacheHelper(
            IRuntimeCacheProvider httpCacheProvider,
            ICacheProvider staticCacheProvider,
            ICacheProvider requestCacheProvider,
            IsolatedRuntimeCache isolatedCacheManager)
        {
            RuntimeCache = httpCacheProvider ?? throw new ArgumentNullException(nameof(httpCacheProvider));
            StaticCache = staticCacheProvider ?? throw new ArgumentNullException(nameof(staticCacheProvider));
            RequestCache = requestCacheProvider ?? throw new ArgumentNullException(nameof(requestCacheProvider));
            IsolatedRuntimeCache = isolatedCacheManager ?? throw new ArgumentNullException(nameof(isolatedCacheManager));
        }

        /// <summary>
        /// Gets the special disabled instance.
        /// </summary>
        /// <remarks>
        /// <para>When used by repositories, all cache policies apply, but the underlying caches do not cache anything.</para>
        /// <para>Used by tests.</para>
        /// </remarks>
        public static CacheHelper Disabled { get; } = new CacheHelper(NullCacheProvider.Instance, NullCacheProvider.Instance, NullCacheProvider.Instance, new IsolatedRuntimeCache(_ => NullCacheProvider.Instance));

        /// <summary>
        /// Gets the special no-cache instance.
        /// </summary>
        /// <remarks>
        /// <para>When used by repositories, all cache policies are bypassed.</para>
        /// <para>Used by repositories that do no cache.</para>
        /// </remarks>
        public static CacheHelper NoCache { get; } = new CacheHelper(NullCacheProvider.Instance, NullCacheProvider.Instance, NullCacheProvider.Instance, new IsolatedRuntimeCache(_ => NullCacheProvider.Instance));

        /// <summary>
        /// Returns the current Request cache
        /// </summary>
        public ICacheProvider RequestCache { get; internal set; }

        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
        public ICacheProvider StaticCache { get; internal set; }

        /// <summary>
        /// Returns the current Runtime cache
        /// </summary>
        public IRuntimeCacheProvider RuntimeCache { get; internal set; }

        /// <summary>
        /// Returns the current Isolated Runtime cache manager
        /// </summary>
        public IsolatedRuntimeCache IsolatedRuntimeCache { get; internal set; }

    }

}
