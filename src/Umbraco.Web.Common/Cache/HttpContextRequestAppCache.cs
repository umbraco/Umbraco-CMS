using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Umbraco.Cms.Core.Events;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Implements a <see cref="IAppCache"/> on top of <see cref="IHttpContextAccessor"/>
    /// </summary>
    /// <remarks>
    /// <para>The HttpContext is not thread safe and no part of it is which means we need to include our own thread
    /// safety mechanisms. This relies on notifications: <see cref="UmbracoRequestBegin"/> and <see cref="UmbracoRequestEnd"/>
    /// in order to facilitate the correct locking and releasing allocations.
    /// </para>
    /// </remarks>
    public class HttpContextRequestAppCache : FastDictionaryAppCacheBase, IRequestCache
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestAppCache"/> class with a context, for unit tests!
        /// </summary>
        public HttpContextRequestAppCache(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

        public bool IsAvailable => TryGetContextItems(out _);

        private bool TryGetContextItems(out IDictionary<object, object> items)
        {
            items = _httpContextAccessor.HttpContext?.Items;
            return items != null;
        }

        /// <inheritdoc />
        public override object Get(string key, Func<object> factory)
        {
            //no place to cache so just return the callback result
            if (!TryGetContextItems(out var items))
            {
                return factory();
            }

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
            if (value is SafeLazy.ExceptionHolder eh)
            {
                eh.Exception.Throw(); // throw once!
            }

            return value;
        }

        public bool Set(string key, object value)
        {
            //no place to cache so just return the callback result
            if (!TryGetContextItems(out var items))
            {
                return false;
            }

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
            if (!TryGetContextItems(out var items))
            {
                return false;
            }

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

            if (!TryGetContextItems(out var items))
                return Enumerable.Empty<KeyValuePair<object, object>>();

            return items.Cast<KeyValuePair<object, object>>()
                .Where(x => x.Key is string s && s.StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            if (!TryGetContextItems(out var items))
                return;

            items.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return !TryGetContextItems(out var items) ? null : items[key];
        }

        #endregion

        #region Lock

        protected override void EnterReadLock()
        {
            object locker = GetLock();
            if (locker == null)
            {
                return;
            }
            Monitor.Enter(locker);
        }

        protected override void EnterWriteLock()
        {
            object locker = GetLock();
            if (locker == null)
            {
                return;
            }
            Monitor.Enter(locker);
        }

        protected override void ExitReadLock()
        {
            object locker = GetLock();
            if (locker == null)
            {
                return;
            }
            if (Monitor.IsEntered(locker))
            {
                Monitor.Exit(locker);
            }
        }

        protected override void ExitWriteLock()
        {
            object locker = GetLock();
            if (locker == null)
            {
                return;
            }
            if (Monitor.IsEntered(locker))
            {
                Monitor.Exit(locker);
            }
        }

        #endregion

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (!TryGetContextItems(out IDictionary<object, object> items))
            {
                yield break;
            }

            foreach (KeyValuePair<object, object> item in items)
            {
                yield return new KeyValuePair<string, object>(item.Key.ToString(), item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Ensures and returns the current lock
        /// </summary>
        /// <returns></returns>
        private object GetLock()
        {
            HttpContext httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            RequestLock requestLock = httpContext.Features.Get<RequestLock>();
            if (requestLock != null)
            {
                return requestLock.SyncRoot;
            }

            IFeatureCollection features = httpContext.Features;
            
            lock (httpContext)
            {
                requestLock = new RequestLock();
                features.Set(requestLock);
                return requestLock.SyncRoot;
            }
        }

        /// <summary>
        /// Used as Scoped instance to allow locking within a request
        /// </summary>
        private class RequestLock
        {
            public object SyncRoot { get; } = new object();
        }
    }
}
