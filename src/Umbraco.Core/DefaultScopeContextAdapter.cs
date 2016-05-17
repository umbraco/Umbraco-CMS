using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Umbraco.Core
{
    internal class DefaultScopeContextAdapter : IScopeContextAdapter
    {
        // note
        // CallContext stuff will flow downwards in async but not upwards ie an async call will receive the values
        // but any changes it makes will *not* modify the caller's CallContext, so we should be careful when using
        // this for caches and stuff
        //
        // also might have to look for another solution for .NET Core as CallContext prob won't be available.

        public object Get(string key)
        {
            return HttpContext.Current == null
                ? CallContext.LogicalGetData(key)
                : HttpContext.Current.Items[key];
        }

        public void Set(string key, object value)
        {
            if (HttpContext.Current == null)
            {
                if (value != null)
                    CallContext.LogicalSetData(key, value);
                else
                    CallContext.FreeNamedDataSlot(key);
            }
            else
            {
                if (value != null)
                    HttpContext.Current.Items[key] = value;
                else
                    HttpContext.Current.Items.Remove(key);
            }
        }

        public void Clear(string key)
        {
            if (HttpContext.Current == null)
                CallContext.FreeNamedDataSlot(key);
            else
                HttpContext.Current.Items.Remove(key);
        }
    }
}
