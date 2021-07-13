using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public interface ITransactableDictionaryFactory<TKey, TValue>
    {
        /// <summary>
        /// Gets an instance of ITransactableDictionary
        /// </summary>
        /// <param name="name">Dictionary name</param>
        /// <param name="keyComparer">Optional comparer for ordering</param>
        /// <param name="isReadOnly">Whether to open as readonly</param>
        /// <param name="enableCount">Whether Count is updated. (Expensive if true)</param>
        /// <returns></returns>
        ITransactableDictionary<TKey, TValue> Get(string name, IComparer<TKey> keyComparer = null, bool isReadOnly = false, bool enableCount = false);
        /// <summary>
        /// Clear out all records
        /// </summary>
        /// <param name="name">Dictionary name</param>
        void Drop(string name);
        /// <summary>
        /// Ensures that the ITransactableDictionaryFactory has the proper environment to run.
        /// </summary>
        /// <param name="errors">The errors, if any.</param>
        /// <returns>A value indicating whether the ITransactableDictionaryFactory has the proper environment to run.</returns>
        bool EnsureEnvironment(out IEnumerable<string> errors);
    }
}
