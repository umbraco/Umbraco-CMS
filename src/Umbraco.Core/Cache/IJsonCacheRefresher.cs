using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache refresher that supports refreshing or removing cache based on a custom Json payload
    /// </summary>
    interface IJsonCacheRefresher : ICacheRefresher
    {
        void Refresh(string jsonPayload);
        void Remove(string jsonPayload);
    }
}