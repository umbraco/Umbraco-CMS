using System;
using System.Collections;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public interface ITransactableDictionary<TKey, TValue> :
        ITransactable,
        //IConcurrentDictionary<TKey, TValue>,
        //IDictionaryEx<TKey, TValue>,
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDisposable
    {
        //IDictionaryEx<TKey, TValue>
        void TryRemove(TKey key, out TValue unused);
    }
    public interface ITransactable : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
