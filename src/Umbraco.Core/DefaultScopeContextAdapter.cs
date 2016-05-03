using System.Runtime.Remoting.Messaging;
using System.Web;

namespace Umbraco.Core
{
    internal class DefaultScopeContextAdapter : IScopeContextAdapter
    {
        
        // some reading:
        // fixme: do we need to keep these links?
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
