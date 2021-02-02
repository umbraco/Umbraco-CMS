using System;
using System.Collections;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public interface ITransactableDictionary<TKey, TValue> :
        ITransactable,
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDisposable
    {
        /// <summary>
        /// Conditionally removes a key/value pair from the dictionary via an implementation
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="value">Removed Value</param>
        /// <returns>Removed</returns>
        bool TryRemove(TKey key, out TValue value);

        /// <summary>
        /// Whether the dictionary has been populated
        /// </summary>
        /// <returns></returns>
        bool IsPopulated();

        /// <summary>
        /// Drop the database
        /// </summary>
        void Drop();
    }
}
