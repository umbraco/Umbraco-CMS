using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache refresher that supports refreshing cache based on a custom payload
    /// </summary>
    interface IPayloadCacheRefresher : IJsonCacheRefresher
    {
        /// <summary>
        /// Refreshes, clears, etc... any cache based on the information provided in the payload
        /// </summary>
        /// <param name="payload"></param>
        void Refresh(object payload);
    }
}
