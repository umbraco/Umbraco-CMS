using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    internal class HttpRequestAppCache : FastDictionaryAppCacheBase
    {
        private readonly HttpContextBase _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestAppCache"/> class with a context, for unit tests!
        /// </summary>
        public HttpRequestAppCache(HttpContextBase context)
        {
            _context = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestAppCache"/> class.
        /// </summary>
        /// <remarks>
        /// <para>Will use HttpContext.Current.</para>
        /// fixme/task: use IHttpContextAccessor NOT HttpContext.Current
        /// </remarks>
        public HttpRequestAppCache()
        { }

        private IDictionary ContextItems => _context?.Items ?? HttpContext.Current?.Items;

        private bool HasContextItems => _context?.Items != null || HttpContext.Current != null;

        /// <inheritdoc />
        public override object Get(string key, Func<object> factory)
        {
            //no place to cache so just return the callback result
            if (HasContextItems == false) return factory();

            key = GetCacheKey(key);

            Lazy<object> result;

            try
            {
                EnterWriteLock();
                result = ContextItems[key] as Lazy<object>; // null if key not found

                // cannot create value within the lock, so if result.IsValueCreated is false, just
                // do nothing here - means that if creation throws, a race condition could cause
                // more than one thread to reach the return statement below and throw - accepted.

                if (result == null || GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = GetSafeLazy(factory);
                    ContextItems[key] = result;
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
            if (value is ExceptionHolder eh) eh.Exception.Throw(); // throw once!
            return value;
        }

        #region Entries

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";

            if (HasContextItems == false) return Enumerable.Empty<DictionaryEntry>();

            return ContextItems.Cast<DictionaryEntry>()
                .Where(x => x.Key is string s && s.StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            if (HasContextItems == false) return;

            ContextItems.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return HasContextItems ? ContextItems[key] : null;
        }

        #endregion

        #region Lock

        private bool _entered;

        protected override void EnterReadLock() => EnterWriteLock();

        protected override void EnterWriteLock()
        {
            if (HasContextItems)
            {
                System.Threading.Monitor.Enter(ContextItems.SyncRoot, ref _entered);
            }
        }

        protected override void ExitReadLock() => ExitWriteLock();

        protected override void ExitWriteLock()
        {
            if (_entered)
            {
                _entered = false;
                System.Threading.Monitor.Exit(ContextItems.SyncRoot);
            }
        }

        #endregion
    }
}
