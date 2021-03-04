using System.Web;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Web.AspNet
{
    public class AspNetSessionManager: ISessionManager, ISessionIdResolver
    {

        public AspNetSessionManager()
        {
        }

        public string GetSessionValue(string sessionName)
        {
            return HttpContext.Current?.Session[sessionName]?.ToString();
        }

        public void SetSessionValue(string sessionName, string value)
        {
            var httpContext = HttpContext.Current;
            if (httpContext is null) return;
            HttpContext.Current.Session[sessionName] = value;
        }

        public string SessionId => HttpContext.Current?.Session?.SessionID;
    }
}
