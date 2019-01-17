using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache provider that caches items in the HttpContext.Items
    /// </summary>
    /// <remarks>
    /// If the Items collection is null, then this provider has no effect
    /// </remarks>
    internal class HttpRequestCacheProvider : DictionaryCacheProviderBase
    {
        // context provider
        // the idea is that there is only one, application-wide HttpRequestCacheProvider instance,
        // that is initialized with a method that returns the "current" context.
        // NOTE
        //   but then it is initialized with () => new HttpContextWrapper(HttpContent.Current)
        //   which is highly inefficient because it creates a new wrapper each time we refer to _context()
        //   so replace it with _context1 and _context2 below + a way to get context.Items.
        //private readonly Func<HttpContextBase> _context;

        // NOTE
        //   and then in almost 100% cases _context2 will be () => HttpContext.Current
        //   so why not bring that logic in here and fallback on to HttpContext.Current when
        //   _context1 is null?
        //private readonly HttpContextBase _context1;
        //private readonly Func<HttpContext> _context2;
        private readonly HttpContextBase _context;

        private IDictionary ContextItems
        {
            //get { return _context1 != null ? _context1.Items : _context2().Items; }
            get { return _context != null ? _context.Items : HttpContext.Current.Items; }
        }

        private bool HasContextItems
        {
            get { return (_context != null && _context.Items != null) || HttpContext.Current != null; }
        }

        // for unit tests
        public HttpRequestCacheProvider(HttpContextBase context)
        {
            _context = context;
        }

        // main constructor
        // will use HttpContext.Current
        public HttpRequestCacheProvider(/*Func<HttpContext> context*/)
        {
            //_context2 = context;
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";

            if (HasContextItems == false) return Enumerable.Empty<DictionaryEntry>();

            return ContextItems.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string)x.Key).StartsWith(prefix));
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

        #region Get

        public override object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            //no place to cache so just return the callback result
            if (HasContextItems == false) return getCacheItem();

            cacheKey = GetCacheKey(cacheKey);

            Lazy<object> result;

            try
            {
                EnterWriteLock();
                result = ContextItems[cacheKey] as Lazy<object>; // null if key not found

                // cannot create value within the lock, so if result.IsValueCreated is false, just
                // do nothing here - means that if creation throws, a race condition could cause
                // more than one thread to reach the return statement below and throw - accepted.

                if (result == null || GetSafeLazyValue(result, true) == null) // get non-created as NonCreatedValue & exceptions as null
                {
                    result = GetSafeLazy(getCacheItem);
                    ContextItems[cacheKey] = result;
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

        #endregion

        #region Insert
        #endregion

        private class NoopLocker : DisposableObjectSlim
        {
            protected override void DisposeResources()
            { }
        }
    }
}
