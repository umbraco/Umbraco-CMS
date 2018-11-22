using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache refresher that supports refreshing or removing cache based on a custom Json payload
    /// </summary>
    interface IJsonCacheRefresher : ICacheRefresher
    {
        /// <summary>
        /// Refreshes, clears, etc... any cache based on the information provided in the json
        /// </summary>
        /// <param name="jsonPayload"></param>
        void Refresh(string jsonPayload);
    }
}