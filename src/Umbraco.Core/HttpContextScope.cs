using System.Web;

namespace Umbraco.Core
{
    /// <summary>
    /// A place to get/retrieve data in a current context (i.e. http, thread, etc...)
    /// </summary>
    internal class HttpContextScope : IScopeContext
    {
        public object GetData(string key)
        {
            return HttpContext.Current.Items[key];
        }

        public void SetData(string key, object data)
        {
            HttpContext.Current.Items[key] = data;
        }

        public void ClearData(string key)
        {
            HttpContext.Current.Items.Remove(key);
        }
    }
}