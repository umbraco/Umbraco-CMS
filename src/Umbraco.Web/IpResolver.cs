using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
    public class IpResolver : IIpResolver
    {
        public string GetCurrentRequestIpAddress()
        {
            var httpContext = HttpContext.Current == null ? (HttpContextBase) null : new HttpContextWrapper(HttpContext.Current);
            var ip = httpContext.GetCurrentRequestIpAddress();
            if (ip.ToLowerInvariant().StartsWith("unknown")) ip = "";
            return ip;
        }
    }
}
