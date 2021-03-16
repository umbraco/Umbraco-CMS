using System.Web;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Web
{
    internal class AspNetIpResolver : IIpResolver
    {
        public string GetCurrentRequestIpAddress()
        {
            var httpContext = HttpContext.Current is null ? null : new HttpContextWrapper(HttpContext.Current);
            var ip = httpContext.GetCurrentRequestIpAddress();
            if (ip.ToLowerInvariant().StartsWith("unknown")) ip = "";
            return ip;
        }
    }
}
