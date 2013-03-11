using umbraco.interfaces;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Strongly type cache refresher that is able to refresh cache of real instances of objects as well as IDs
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This is much better for performance when we're not running in a load balanced environment so we can refresh the cache
    /// against a already resolved object instead of looking the object back up by id. 
    /// </remarks>
    interface ICacheRefresher<T> : ICacheRefresher
    {
        void Refresh(T instance);
        void Remove(T instance);
    }
}