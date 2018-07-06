namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Represent disabled application-wide caches.
    /// </summary>
    public class DisabledCacheHelper : CacheHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisabledCacheHelper"/>.
        /// </summary>
        public DisabledCacheHelper()
            : base(NullCacheProvider.Instance, NullCacheProvider.Instance, NullCacheProvider.Instance, new IsolatedRuntimeCache(_ => NullCacheProvider.Instance))
        { }
    }
}