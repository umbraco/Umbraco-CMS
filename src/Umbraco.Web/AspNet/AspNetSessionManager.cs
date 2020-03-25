using System.Web;
using Umbraco.Core.Net;
using Umbraco.Core.Session;

namespace Umbraco.Web.AspNet
{
    public class AspNetSessionManager: ISessionManager, ISessionIdResolver
    {

        public AspNetSessionManager()
        {
        }

        public object GetSessionValue(string sessionName)
        {
            return HttpContext.Current.Session[sessionName];
        }

        public void SetSessionValue(string sessionName, object value)
        {
            HttpContext.Current.Session[sessionName] = value;
        }

        public string SessionId => HttpContext.Current?.Session?.SessionID;
    }
}
