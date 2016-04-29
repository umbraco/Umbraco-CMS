using System.Runtime.Remoting.Messaging;

namespace Umbraco.Core
{
    /// <summary>
    /// A place to get/retrieve data in a current context (i.e. http, thread, etc...)
    /// </summary>
    internal class CallContextScope : IScopeContext
    {
        public object GetData(string key)
        {
            return CallContext.GetData(key);
        }

        public void SetData(string key, object data)
        {
            CallContext.SetData(key, data);
        }

        public void ClearData(string key)
        {
            CallContext.FreeNamedDataSlot(key);
        }
    }
}