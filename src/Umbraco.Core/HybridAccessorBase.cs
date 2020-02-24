﻿using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Scoping;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides a base class for hybrid accessors.
    /// </summary>
    /// <typeparam name="T">The type of the accessed object.</typeparam>
    /// <remarks>
    /// <para>Hybrid accessors store the accessed object in HttpContext if they can,
    /// otherwise they rely on the logical call context, to maintain an ambient
    /// object that flows with async.</para>
    /// </remarks>
    public abstract class HybridAccessorBase<T>
        where T : class
    {
        private readonly IRequestCache _requestCache;

        // ReSharper disable StaticMemberInGenericType
        private static readonly object Locker = new object();
        private static bool _registered;
        // ReSharper restore StaticMemberInGenericType

        protected abstract string ItemKey { get; }

        // read
        // http://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html
        // http://stackoverflow.com/questions/14176028/why-does-logicalcallcontext-not-work-with-async
        // http://stackoverflow.com/questions/854976/will-values-in-my-threadstatic-variables-still-be-there-when-cycled-via-threadpo
        // https://msdn.microsoft.com/en-us/library/dd642243.aspx?f=255&MSPPError=-2147217396 ThreadLocal<T>
        // http://stackoverflow.com/questions/29001266/cleaning-up-callcontext-in-tpl clearing call context
        //
        // anything that is ThreadStatic will stay with the thread and NOT flow in async threads
        // the only thing that flows is the logical call context (safe in 4.5+)

        // no!
        //[ThreadStatic]
        //private static T _value;

        // yes! flows with async!
        private T NonContextValue
        {
            get => CallContext<T>.GetData(ItemKey);
            set
            {
                CallContext<T>.SetData(ItemKey, value);
            }
        }

        protected HybridAccessorBase(IRequestCache requestCache)
        {
            _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));

            lock (Locker)
            {
                // register the itemKey once with SafeCallContext
                if (_registered) return;
                _registered = true;
            }

            // ReSharper disable once VirtualMemberCallInConstructor
            var itemKey = ItemKey; // virtual
            SafeCallContext.Register(() =>
            {
                var value = CallContext<T>.GetData(itemKey);
                return value;
            }, o =>
            {
                if (o == null) return;
                var value = o as T;
                if (value == null) throw new ArgumentException($"Expected type {typeof(T).FullName}, got {o.GetType().FullName}", nameof(o));
                CallContext<T>.SetData(itemKey, value);
            });
        }

        protected T Value
        {
            get
            {
                if (!_requestCache.IsAvailable)
                {
                    return NonContextValue;
                }
                return (T) _requestCache.Get(ItemKey);
            }

            set
            {
                if (!_requestCache.IsAvailable)
                {
                    NonContextValue = value;
                }
                else if (value == null)
                    _requestCache.Remove(ItemKey);
                else
                    _requestCache.Set(ItemKey, value);
            }
        }
    }
}
