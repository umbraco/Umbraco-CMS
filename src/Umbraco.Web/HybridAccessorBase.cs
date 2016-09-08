using System.Runtime.Remoting.Messaging;

namespace Umbraco.Web
{
    internal abstract class HybridAccessorBase<T>
    {
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
        // now...
        // tests seem to show that either newing Thread or enqueuing in the ThreadPool both produce a thread
        // with a clear logical call context, which would mean that it is somewhat safe to "fire and forget"
        // because whatever is in the call context will be gone when the thread returns to the pool

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
            _httpContextAccessor = httpContextAccessor;
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
                {
                    NonContextValue = value;
                    return;
                }
                httpContext.Items[ItemKey] = value;
            }
        }
    }
}