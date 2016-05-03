using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Umbraco.Core
{
    internal class DefaultScopeContextAdapter : IScopeContextAdapter
    {
        // fixme - should we use the LogicalCallContext here?

        public object Get(string key)
        {
            return HttpContext.Current == null
                ? CallContext.GetData(key)
                : HttpContext.Current.Items[key];
        }

        public void Set(string key, object value)
        {
            if (HttpContext.Current == null)
            {
                if (value != null)
                    CallContext.SetData(key, value);
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
