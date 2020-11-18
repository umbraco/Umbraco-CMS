using System;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public static class OwinExtensions
    {

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
        /// Gets the back office user manager out of OWIN
        /// </summary>
        /// <param name="owinContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is required because to extract the user manager we need to user a custom service since owin only deals in generics and
        /// developers could register their own user manager types
        /// </remarks>
        public static BackOfficeOwinUserManager GetBackOfficeUserManager(this IOwinContext owinContext)
        {
            var marker = owinContext.Get<IBackOfficeUserManagerMarker>(BackOfficeOwinUserManager.OwinMarkerKey)
                ?? throw new NullReferenceException($"No {typeof (IBackOfficeUserManagerMarker)}, i.e. no Umbraco back-office, has been registered with Owin.");

            return marker.GetManager(owinContext)
                ?? throw new NullReferenceException($"Could not resolve an instance of {typeof (BackOfficeOwinUserManager)} from the {typeof (IOwinContext)}.");
        }

        /// <summary>
        /// Adapted from Microsoft.AspNet.Identity.Owin.OwinContextExtensions
        /// </summary>
        public static T Get<T>(this IOwinContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Get<T>(GetKey(typeof(T)));
        }

        public static IOwinContext Set<T>(this IOwinContext context, T value)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Set(GetKey(typeof(T)), value);
        }

        private static string GetKey(Type t)
        {
            return "AspNet.Identity.Owin:" + t.AssemblyQualifiedName;
        }
    }
}
