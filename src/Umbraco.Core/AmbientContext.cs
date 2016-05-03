//#define CALLCONTEXT
#define LOGICALCONTEXT

using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Umbraco.Core
{
    // fixme - not used at the moment, instead we have the IScopeContextAdapter thing
    //
    // the scope context adapter needs to be created and passed around... whereas... this "just exists"
    // and it's possible to do AmbientContext.Get(key) just anywhere and it will use the "ambient" context,
    // which is based upon HttpContext in the context of a web request, or CallContext otherwise.
    //
    // read:
    // https://github.com/seesharper/LightInject/issues/170
    // http://stackoverflow.com/questions/22924048/is-it-possible-to-detect-if-you-are-on-the-synchronous-side-of-an-async-method
    // http://stackoverflow.com/questions/29917671/lightinject-with-web-api-how-can-i-get-the-httprequestmessage
    // http://stackoverflow.com/questions/18459916/httpcontext-current-items-after-an-async-operation
    // http://stackoverflow.com/questions/12029091/httpcontext-is-null-after-await-task-factory-fromasyncbeginxxx-endxxx
    //
    // also note
    // CallContext stuff will flow downwards in async but not upwards ie an async call will receive the values
    // but any changes it makes will *not* modify the caller's CallContext, so we should be careful when using
    // this for caches and stuff

    internal static class AmbientContext
    {
        /// <summary>
        /// Gets or sets the values container.
        /// </summary>
        /// <remarks>For unit tests EXCLUSIVELY.</remarks>
        internal static IDictionary<string, object> Values { get; set; }

        public static object Get(string key)
        {
            if (Values != null)
            {
                object value;
                return Values.TryGetValue(key, out value) ? value : null;
            }

            return HttpContext.Current == null
#if CALLCONTEXT
                ? CallContext.GetData(key)
#elif LOGICALCONTEXT
                ? CallContext.LogicalGetData(key)
#endif
                : HttpContext.Current.Items[key];
        }

        public static void Set(string key, object value)
        {
            if (Values != null)
            {
                if (value != null)
                    Values[key] = value;
                else
                    Values.Remove(key);
                return;
            }

            if (HttpContext.Current == null)
            {
                if (value != null)
#if CALLCONTEXT
                    CallContext.SetData(key, value);
#elif LOGICALCONTEXT
                    CallContext.LogicalSetData(key, value);
#endif
                else
#if CALLCONTEXT || LOGICALCONTEXT
                    CallContext.FreeNamedDataSlot(key); // clears both
#endif
            }
            else
            {
                if (value != null)
                    HttpContext.Current.Items[key] = value;
                else
                    HttpContext.Current.Items.Remove(key);
            }
        }

        public static void Clear(string key)
        {
            if (Values != null)
            {
                Values.Remove(key);
                return;
            }

            if (HttpContext.Current == null)
#if CALLCONTEXT || LOGICALCONTEXT
                CallContext.FreeNamedDataSlot(key); // clears both
#endif
            else
                HttpContext.Current.Items.Remove(key);
        }
    }
}
