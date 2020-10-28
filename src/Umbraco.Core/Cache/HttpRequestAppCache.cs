using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements a fast <see cref="IAppCache"/> on top of HttpContext.Items.
    /// </summary>
    /// <remarks>
    /// <para>If no current HttpContext items can be found (no current HttpContext,
    /// or no Items...) then this cache acts as a pass-through and does not cache
    /// anything.</para>
    /// </remarks>
    [UmbracoVolatile]
    public class HttpRequestAppCache : FastDictionaryAppCacheBase, IRequestCache
    {
        private static object _syncRoot = new object(); // Using this for locking as the SyncRoot property is not available to us
                                                        // on the provided collection provided from .NET Core's HttpContext.Items dictionary,
                                                        // as it doesn't implement ICollection where SyncRoot is defined.

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestAppCache"/> class with a context, for unit tests!
        /// </summary>
        public HttpRequestAppCache(Func<IDictionary<object, object>> requestItems) : base()
        {
            ContextItems = requestItems;
        }

        private Func<IDictionary<object, object>> ContextItems { get; }

        public bool IsAvailable => TryGetContextItems(out _);

        private bool TryGetContextItems(out IDictionary<object, object> items)
        {
            items = ContextItems?.Invoke();
            return items != null;
        }

        /// <inheritdoc />
        public override object Get(string key, Func<object> factory)
        {
            //no place to cache so just return the callback result
            if (!TryGetContextItems(out var items)) return factory();

            key = GetCacheKey(key);

            Lazy<object> result;

            try
            {
                EnterWriteLock();
                result = items[key] as Lazy<object>; // null if key not found

                // cannot create value within the lock, so if result.IsValueCreated is false, just
                // do nothing here - means that if creation throws, a race condition could cause
                // more than one thread to reach the return statement below and throw - accepted.

                if (result == null || SafeLazy.GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = SafeLazy.GetSafeLazy(factory);
                    items[key] = result;
                }
            }
            finally
            {
                ExitWriteLock();
            }

            // using GetSafeLazy and GetSafeLazyValue ensures that we don't cache
            // exceptions (but try again and again) and silently eat them - however at
            // some point we have to report them - so need to re-throw here

            // this does not throw anymore
            //return result.Value;

            var value = result.Value; // will not throw (safe lazy)
            if (value is SafeLazy.ExceptionHolder eh) eh.Exception.Throw(); // throw once!
            return value;
        }

        public bool Set(string key, object value)
        {
            //no place to cache so just return the callback result
            if (!TryGetContextItems(out var items)) return false;
            key = GetCacheKey(key);
            try
            {

                EnterWriteLock();
                items[key] = SafeLazy.GetSafeLazy(() => value);
            }
            finally
            {
                ExitWriteLock();
            }
            return true;
        }

        public bool Remove(string key)
        {
            //no place to cache so just return the callback result
            if (!TryGetContextItems(out var items)) return false;
            key = GetCacheKey(key);
            try
            {

                EnterWriteLock();
                items.Remove(key);
            }
            finally
            {
                ExitWriteLock();
            }
            return true;
        }

        #region Entries

        protected override IEnumerable<KeyValuePair<object, object>> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";

            if (!TryGetContextItems(out var items)) return Enumerable.Empty<KeyValuePair<object, object>>();

            return items.Cast<KeyValuePair<object, object>>()
                .Where(x => x.Key is string s && s.StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            if (!TryGetContextItems(out var items)) return;

            items.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return !TryGetContextItems(out var items) ? null : items[key];
        }

        #endregion

        #region Lock

        private const string ContextItemsLockKey = "Umbraco.Core.Cache.HttpRequestCache::LockEntered";

        protected override void EnterReadLock() => EnterWriteLock();

        protected override void EnterWriteLock()
        {
            if (!TryGetContextItems(out var items)) return;

            // note: cannot keep 'entered' as a class variable here,
            // since there is one per request - so storing it within
            // ContextItems - which is locked, so this should be safe

            var entered = false;
            Monitor.Enter(_syncRoot, ref entered);
            items[ContextItemsLockKey] = entered;
        }

        protected override void ExitReadLock() => ExitWriteLock();

        protected override void ExitWriteLock()
        {
            if (!TryGetContextItems(out var items)) return;

            var entered = (bool?)items[ContextItemsLockKey] ?? false;
            if (entered)
                Monitor.Exit(_syncRoot);
            items.Remove(ContextItemsLockKey);
        }

        #endregion

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (!TryGetContextItems(out var items))
            {
                yield break;
            }

            foreach (var item in items)
            {
                yield return new KeyValuePair<string, object>(item.Key.ToString(), item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
