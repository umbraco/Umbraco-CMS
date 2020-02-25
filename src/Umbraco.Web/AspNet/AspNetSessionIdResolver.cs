using System.Web;
using Umbraco.Core;
using Umbraco.Net;

namespace Umbraco.Web
{
    internal class AspNetSessionIdResolver : ISessionIdResolver
    {
        public string SessionId => HttpContext.Current?.Session?.SessionID;
    }
}
