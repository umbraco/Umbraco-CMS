using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements <see cref="IAppCache"/> on top of a concurrent dictionary.
    /// </summary>
    public class DictionaryCacheProvider : IAppCache
    {
        /// <summary>
        /// Gets the internal items dictionary, for tests only!
        /// </summary>
        internal readonly ConcurrentDictionary<string, object> Items = new ConcurrentDictionary<string, object>();

        /// <inheritdoc />
        public virtual object Get(string key)
        {
            // fixme throws if non-existing, shouldn't it return null?
            return Items[key];
        }

        /// <inheritdoc />
        public virtual object Get(string key, Func<object> factory)
        {
            return Items.GetOrAdd(key, _ => factory());
        }

        /// <inheritdoc />
        public virtual IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            var items = new List<object>();
            foreach (var (key, value) in Items)
                if (key.InvariantStartsWith(keyStartsWith))
                    items.Add(value);
            return items;
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            var items = new List<object>();
            foreach (var (key, value) in Items)
                if (compiled.IsMatch(key))
                    items.Add(value);
            return items;
        }

        /// <inheritdoc />
        public virtual void Clear()
        {
            Items.Clear();
        }

        /// <inheritdoc />
        public virtual void Clear(string key)
        {
            Items.TryRemove(key, out _);
        }

        /// <inheritdoc />
        public virtual void ClearOfType(string typeName)
        {
            Items.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType().ToString().InvariantEquals(typeName));
        }

        /// <inheritdoc />
        public virtual void ClearOfType<T>()
        {
            var typeOfT = typeof(T);
            Items.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType() == typeOfT);
        }

        /// <inheritdoc />
        public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            Items.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType() == typeOfT && predicate(kvp.Key, (T)kvp.Value));
        }

        /// <inheritdoc />
        public virtual void ClearByKey(string keyStartsWith)
        {
            Items.RemoveAll(kvp => kvp.Key.InvariantStartsWith(keyStartsWith));
        }

        /// <inheritdoc />
        public virtual void ClearByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            Items.RemoveAll(kvp => compiled.IsMatch(kvp.Key));
        }
    }
}
