using System;
using System.Collections;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public interface ITransactableDictionary<TKey, TValue> :
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
        /// Clears the contents of the database.
        /// </summary>
        void Drop();


        /// <summary>
        /// Begin Transaction
        /// </summary>
        /// <returns>Transaction Scope</returns>
        ITransactionScope BeginTransaction();

        /// <summary>
        /// Initialize
        /// </summary>
        void Init();

        /// <summary>
        /// Ensures that the ITransactableDictionaryFactory has the proper environment to run.
        /// </summary>
        /// <param name="errors">The errors, if any.</param>
        /// <returns>A value indicating whether the ITransactableDictionaryFactory has the proper environment to run.</returns>
        bool EnsureEnvironment(out IEnumerable<string> errors);
    }
}
