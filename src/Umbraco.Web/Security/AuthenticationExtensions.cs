using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration.Models;
using Umbraco.Extensions;
using Umbraco.Web.Composing;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Extensions to create and renew and remove authentication tickets for the Umbraco back office
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// This will check the ticket to see if it is valid, if it is it will set the current thread's user and culture
        /// </summary>
        /// <param name="http"></param>
        /// <param name="ticket"></param>
        /// <param name="renewTicket">If true will attempt to renew the ticket</param>
        public static bool AuthenticateCurrentRequest(this HttpContextBase http, AuthenticationTicket ticket, bool renewTicket)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));

            // if there was a ticket, it's not expired, - it should not be renewed or its renewable
            if (ticket?.Properties.ExpiresUtc != null && ticket.Properties.ExpiresUtc.Value > DateTimeOffset.UtcNow && (renewTicket == false || http.RenewUmbracoAuthTicket()))
            {
                try
                {
                    // get the Umbraco user identity
                    if (!(ticket.Identity is UmbracoBackOfficeIdentity identity))
                        throw new InvalidOperationException("The AuthenticationTicket specified does not contain the correct Identity type");

                    // set the principal object
                    var principal = new ClaimsPrincipal(identity);

                    // It is actually not good enough to set this on the current app Context and the thread, it also needs
                    // to be set explicitly on the HttpContext.Current !! This is a strange web api thing that is actually
                    // an underlying fault of asp.net not propagating the User correctly.
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = principal;
                    }
                    http.User = principal;
                    Thread.CurrentPrincipal = principal;

                    // This is a back office request, we will also set the culture/ui culture
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture =
                        new System.Globalization.CultureInfo(identity.Culture);

                    return true;
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is JsonReaderException)
                    {
                        // this will occur if the cookie data is invalid

                    }
                    else
                    {
                        throw;
                    }

                }
            }

            return false;
        }


        /// <summary>
        /// This will return the current back office identity.
        /// </summary>
        /// <param name="http"></param>
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        public static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));
            if (http.User == null) return null; //there's no user at all so no identity

            // If it's already a UmbracoBackOfficeIdentity
            var backOfficeIdentity = http.User.GetUmbracoIdentity();
            if (backOfficeIdentity != null) return backOfficeIdentity;

            return null;
        }

        /// <summary>
        /// This will return the current back office identity.
        /// </summary>
        /// <param name="http"></param>
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        internal static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException("http");
            return new HttpContextWrapper(http).GetCurrentIdentity();
        }

        /// <summary>
        /// This will force ticket renewal in the OWIN pipeline
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool RenewUmbracoAuthTicket(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");
            http.Items[Constants.Security.ForceReAuthFlag] = true;
            return true;
        }

        // NOTE: Migrated to netcore (though in a different way)
        public static double GetRemainingAuthSeconds(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));
            var ticket = http.GetUmbracoAuthTicket();
            return ticket.GetRemainingAuthSeconds();
        }

        // NOTE: Migrated to netcore (though in a different way)
        public static double GetRemainingAuthSeconds(this AuthenticationTicket ticket)
        {
            var utcExpired = ticket?.Properties.ExpiresUtc;
            if (utcExpired == null) return 0;
            var secondsRemaining = utcExpired.Value.Subtract(DateTimeOffset.UtcNow).TotalSeconds;
            return secondsRemaining;
        }

        /// <summary>
        /// Gets the umbraco auth ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static AuthenticationTicket GetUmbracoAuthTicket(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));
            return GetAuthTicket(http, /*Current.Configs.Security() TODO*/new SecuritySettings().AuthCookieName);
        }

        internal static AuthenticationTicket GetUmbracoAuthTicket(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException(nameof(http));
            return new HttpContextWrapper(http).GetUmbracoAuthTicket();
        }

        public static AuthenticationTicket GetUmbracoAuthTicket(this IOwinContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            return GetAuthTicket(ctx, /*Current.Configs.Security() TODO introduce injection instead of default value*/new SecuritySettings().AuthCookieName);
        }

        private static AuthenticationTicket GetAuthTicket(this IOwinContext owinCtx, string cookieName)
        {
            var asDictionary = new Dictionary<string, string>();
            foreach (var requestCookie in owinCtx.Request.Cookies)
            {
                var key = requestCookie.Key;
                asDictionary[key] = requestCookie.Value;
            }

            var secureFormat = owinCtx.GetUmbracoAuthTicketDataProtector();

            // get the ticket
            try
            {

                return GetAuthTicket(secureFormat, asDictionary, cookieName);
            }
            catch (Exception)
            {
                owinCtx.Authentication.SignOut(
                    Constants.Security.BackOfficeAuthenticationType,
                    Constants.Security.BackOfficeExternalAuthenticationType);
                return null;
            }
        }

        private static AuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var asDictionary = new Dictionary<string, string>();
            for (var i = 0; i < http.Request.Cookies.Keys.Count; i++)
            {
                var key = http.Request.Cookies.Keys.Get(i);
                asDictionary[key] = http.Request.Cookies[key].Value;
            }

            var owinCtx = http.GetOwinContext();
            var secureFormat = owinCtx.GetUmbracoAuthTicketDataProtector();

            // will only happen in tests
            if (secureFormat == null) return null;

            // get the ticket
            try
            {
                return GetAuthTicket(secureFormat, asDictionary, cookieName);
            }
            catch (Exception)
            {
                // occurs when decryption fails

                return null;
            }
        }

        private static AuthenticationTicket GetAuthTicket(ISecureDataFormat<AuthenticationTicket> secureDataFormat, IDictionary<string, string> cookies, string cookieName)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));

            if (cookies.ContainsKey(cookieName) == false) return null;

            var formsCookie = cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            // get the ticket

            return secureDataFormat.Unprotect(formsCookie);
        }


    }
}
