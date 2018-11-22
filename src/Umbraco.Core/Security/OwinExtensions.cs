using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public static class OwinExtensions
    {
        public static string GetCurrentRequestIpAddress(this IOwinContext owinContext)
        {
            if (owinContext == null)
            {
                return "Unknown, owinContext is null";
            }
            if (owinContext.Request == null)
            {
                return "Unknown, owinContext.Request is null";
            }

            var httpContext = owinContext.TryGetHttpContext();
            if (httpContext == false)
            {
                return "Unknown, cannot resolve HttpContext from owinContext";
            }

            return httpContext.Result.GetCurrentRequestIpAddress();
        }

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

        /// <summary>
        /// Gets the back office sign in manager out of OWIN
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>        
        public static BackOfficeSignInManager GetBackOfficeSignInManager(this IOwinContext owinContext)
        {
            var mgr = owinContext.Get<BackOfficeSignInManager>();
            if (mgr == null)
            {
                throw new NullReferenceException("Could not resolve an instance of " + typeof(BackOfficeSignInManager) + " from the " + typeof(IOwinContext));
            }
            return mgr;
        }

        /// <summary>
        /// Gets the back office user manager out of OWIN
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is required because to extract the user manager we need to user a custom service since owin only deals in generics and 
        /// developers could register their own user manager types
        /// </remarks> 
        public static BackOfficeUserManager<BackOfficeIdentityUser> GetBackOfficeUserManager(this IOwinContext owinContext)
        {
            var marker = owinContext.Get<IBackOfficeUserManagerMarker>(BackOfficeUserManager.OwinMarkerKey);
            if (marker == null) throw new NullReferenceException("No " + typeof(IBackOfficeUserManagerMarker) + " has been registered with Owin which means that no Umbraco back office user manager has been registered");

            var mgr = marker.GetManager(owinContext);
            if (mgr == null)
            {
                throw new NullReferenceException("Could not resolve an instance of " + typeof(BackOfficeUserManager<BackOfficeIdentityUser>));
            }
            return mgr;
        }
    }
}