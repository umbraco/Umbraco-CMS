using System;
using System.Collections;
using System.Collections.Generic;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public interface ITransactableDictionary<TKey, TValue> :
        ITransactable,
        CSharpTest.Net.Collections.IConcurrentDictionary<TKey, TValue>,
        IDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDisposable
    {
    }
    public interface ITransactable : IDisposable
    {
        void Commit();
        void Rollback();
    }
}
