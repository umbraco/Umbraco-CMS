using System.Web;
using Microsoft.Owin;

namespace Umbraco.Web.Security.Identity
{
    internal static class OwinExtensions
    {
        /// <summary>
        /// Nasty little hack to get httpcontextbase from an owin context
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        public static HttpContextBase HttpContextFromOwinContext(this IOwinContext owinContext)
        {
            return owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
        }

    }
}