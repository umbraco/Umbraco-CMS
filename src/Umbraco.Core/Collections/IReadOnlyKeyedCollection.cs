using System.Collections.Generic;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// A readonly keyed collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyKeyedCollection<TKey, TVal> : IReadOnlyList<TVal>
    {
        IEnumerable<TKey> Keys { get; }
        bool TryGetValue(TKey key, out TVal val);
        TVal this[string key] { get; }
        bool Contains(TKey key);
    }
}
