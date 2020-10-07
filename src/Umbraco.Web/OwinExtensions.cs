using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Core.Models.Identity;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public static class OwinExtensions
    {
        /// <summary>
        /// Used by external login providers to set any errors that occur during the OAuth negotiation
        /// </summary>
        /// <param name="owinContext"></param>
        /// <param name="errors"></param>
        public static void SetExternalLoginProviderErrors(this IOwinContext owinContext, BackOfficeExternalLoginProviderErrors errors)
            => owinContext.Set(errors);

        /// <summary>
        /// Retrieve any errors set by external login providers during OAuth negotiation
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        internal static BackOfficeExternalLoginProviderErrors GetExternalLoginProviderErrors(this IOwinContext owinContext)
            => owinContext.Get<BackOfficeExternalLoginProviderErrors>();

        /// <summary>
        /// Gets the <see cref="ISecureDataFormat{AuthenticationTicket}"/> for the Umbraco back office cookie
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        internal static ISecureDataFormat<AuthenticationTicket> GetUmbracoAuthTicketDataProtector(this IOwinContext owinContext)
        {
            var found = owinContext.Get<UmbracoAuthTicketDataProtector>();
            return found?.Protector;
        }

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
        /// Nasty little hack to get HttpContextBase from an owin context
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
            return owinContext.Get<BackOfficeSignInManager>()
                ?? throw new NullReferenceException($"Could not resolve an instance of {typeof (BackOfficeSignInManager)} from the {typeof(IOwinContext)}.");
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
            var marker = owinContext.Get<IBackOfficeUserManagerMarker>(BackOfficeUserManager.OwinMarkerKey)
                ?? throw new NullReferenceException($"No {typeof (IBackOfficeUserManagerMarker)}, i.e. no Umbraco back-office, has been registered with Owin.");

            return marker.GetManager(owinContext)
                ?? throw new NullReferenceException($"Could not resolve an instance of {typeof (BackOfficeUserManager<BackOfficeIdentityUser>)} from the {typeof (IOwinContext)}.");
        }
    }

}
