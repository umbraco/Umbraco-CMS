using System;
using System.Runtime.Remoting.Messaging;
using Umbraco.Core;

namespace Umbraco.Web
{
    internal abstract class HybridAccessorBase<T>
        where T : class
    {
        // ReSharper disable StaticMemberInGenericType
        private static readonly object Locker = new object();
        private static bool _registered;
        // ReSharper restore StaticMemberInGenericType

        private readonly IHttpContextAccessor _httpContextAccessor;

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
            get { return (T) CallContext.LogicalGetData(ItemKey); }
            set
            {
                if (value == null) CallContext.FreeNamedDataSlot(ItemKey);
                else CallContext.LogicalSetData(ItemKey, value);
            }
        }

        protected HybridAccessorBase(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));
            _httpContextAccessor = httpContextAccessor;

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
                var value = CallContext.LogicalGetData(itemKey);
                CallContext.FreeNamedDataSlot(itemKey);
                return value;
            }, o =>
            {
                if (o == null) return;
                var value = o as T;
                if (value == null) throw new ArgumentException($"Expected type {typeof(T).FullName}, got {o.GetType().FullName}", nameof(o));
                CallContext.LogicalSetData(itemKey, value);
            });
        }

        protected T Value
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return NonContextValue;
                return (T) httpContext.Items[ItemKey];
            }

            set
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    NonContextValue = value;
                else if (value == null)
                    httpContext.Items.Remove(ItemKey);
                else
                    httpContext.Items[ItemKey] = value;
            }
        }
    }
}
