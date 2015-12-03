using System.Web;
using Microsoft.Owin;
using Umbraco.Core;

namespace Umbraco.Web.Security
{
    internal static class OwinExtensions
    {

        /// <summary>
        /// Nasty little hack to get httpcontextbase from an owin context
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        internal static Attempt<HttpContextBase> TryGetHttpContext(this IOwinContext owinContext)
        {
            var ctx = owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
            return ctx == null ? Attempt<HttpContextBase>.Fail() : Attempt.Succeed(ctx);
        }

    }
}