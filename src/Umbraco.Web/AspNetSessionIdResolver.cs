using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
    internal class AspNetSessionIdResolver : ISessionIdResolver
    {
        public string SessionId => HttpContext.Current?.Session?.SessionID;
    }
}
