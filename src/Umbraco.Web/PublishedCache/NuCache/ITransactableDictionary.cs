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
        /// Presorts the provided enumeration in batches and then performs an optimized insert on the resulting set(s).
        /// </summary>
        /// <param name="unorderedItems">The items to insert</param>
        /// <param name="allowUpdates">True to overwrite any existing records</param>
        /// <returns>The total number of records inserted or updated</returns>
        public int AddRange(IEnumerable<KeyValuePair<TKey, TValue>> unorderedItems, bool allowUpdates);
        /// <summary>
        ///     Optimized insert of presorted key/value pairs. If the input is not presorted,
        ///     please use AddRange() instead.
        /// </summary>
        /// <param name="items">The ordered list of items to insert</param>
        /// <returns>The total number of records inserted or updated</returns>
        public int AddRangeSorted(IEnumerable<KeyValuePair<TKey, TValue>> items, bool allowUpdates = false);

        /// <summary>
        /// Inclusivly enumerates from start key to the end of the collection
        /// </summary>
        /// <param name="start">Starting Key</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> EnumerateFrom(TKey start);

        /// <summary>
        /// Inclusivly enumerates from start key to stop key
        /// </summary>
        /// <param name="start">Starting Key</param>
        /// <param name="end">Stop Key</param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<TKey, TValue>> EnumerateRange(TKey start, TKey end);

        /// <summary>
        /// Returns the first key and it's associated value.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetFirst(out KeyValuePair<TKey, TValue> item);

        /// <summary>
        /// Returns the last key and it's associated value.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetLast(out KeyValuePair<TKey, TValue> item);

        /// <summary>
        /// Count. Count is inaccurate unless <see cref="IsCountEnabled"/>
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Whether Count is enabled
        /// </summary>
        public bool IsCountEnabled { get; }

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
