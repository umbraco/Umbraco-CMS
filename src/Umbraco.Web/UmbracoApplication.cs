using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Runtime;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the Umbraco global.asax class.
    /// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
    {
        protected override IRuntime GetRuntime()
        {
            return new WebRuntime(this);
        }

        /// <summary>
        /// Restarts the Umbraco application.
        /// </summary>
        public static void Restart()
        {
            // see notes in overload

            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Application.Add("AppPoolRestarting", true);
                httpContext.User = null;
            }
            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();
        }

        /// <summary>
        /// Restarts the Umbraco application.
        /// </summary>
        public static void Restart(HttpContextBase httpContext)
        {
            if (httpContext != null)
            {
                // we're going to put an application wide flag to show that the application is about to restart.
                // we're doing this because if there is a script checking if the app pool is fully restarted, then
                // it can check if this flag exists...  if it does it means the app pool isn't restarted yet.
                httpContext.Application.Add("AppPoolRestarting", true);

                // unload app domain - we must null out all identities otherwise we get serialization errors
                // http://www.zpqrtbnk.net/posts/custom-iidentity-serialization-issue
                httpContext.User = null;
            }

            if (HttpContext.Current != null)
                HttpContext.Current.User = null;

            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();
        }
    }
}
