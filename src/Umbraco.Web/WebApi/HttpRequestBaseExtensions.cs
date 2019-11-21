using System.Linq;
using System.Web;

namespace Umbraco.Web.WebApi
{
    internal static class HttpRequestBaseExtensions
    {
        internal static string ClientCulture(this HttpRequestBase request)
        {
            return request.Headers.Get("X-UMB-CULTURE")?.Split(',').FirstOrDefault();
        }
    }
}
