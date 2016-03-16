using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using AutoMapper;
using Microsoft.Owin;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Security
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
        public static bool AuthenticateCurrentRequest(this HttpContextBase http, FormsAuthenticationTicket ticket, bool renewTicket)
        {
            if (http == null) throw new ArgumentNullException("http");

            //if there was a ticket, it's not expired, - it should not be renewed or its renewable
            if (ticket != null && ticket.Expired == false && (renewTicket == false || http.RenewUmbracoAuthTicket()))
            {
                try
                {
                    //create the Umbraco user identity 
                    var identity = new UmbracoBackOfficeIdentity(ticket);

                    //set the principal object
                    var principal = new GenericPrincipal(identity, identity.Roles);

                    //It is actually not good enough to set this on the current app Context and the thread, it also needs
                    // to be set explicitly on the HttpContext.Current !! This is a strange web api thing that is actually 
                    // an underlying fault of asp.net not propogating the User correctly.
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.User = principal;
                    }
                    http.User = principal;
                    Thread.CurrentPrincipal = principal;

                    //This is a back office request, we will also set the culture/ui culture
                    Thread.CurrentThread.CurrentCulture =
                        Thread.CurrentThread.CurrentUICulture =
                        new System.Globalization.CultureInfo(identity.Culture);

                    return true;
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is JsonReaderException)
                    {
                        //this will occur if the cookie data is invalid
                        http.UmbracoLogout();
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
        /// <param name="authenticateRequestIfNotFound"> 
        /// If set to true and a back office identity is not found and not authenticated, this will attempt to authenticate the 
        /// request just as is done in the Umbraco module and then set the current identity if it is valid.
        /// Just like in the UmbracoModule, if this is true then the user's culture will be assigned to the request.
        /// </param>
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        public static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContextBase http, bool authenticateRequestIfNotFound)
        {
            if (http == null) throw new ArgumentNullException("http");
            if (http.User == null) return null; //there's no user at all so no identity

            //If it's already a UmbracoBackOfficeIdentity
            var backOfficeIdentity = http.User.Identity as UmbracoBackOfficeIdentity;
            if (backOfficeIdentity != null) return backOfficeIdentity;

            //Check if there's more than one identity assigned and see if it's a UmbracoBackOfficeIdentity and use that
            var claimsPrincipal = http.User as ClaimsPrincipal;
            if (claimsPrincipal != null)
            {
                backOfficeIdentity = claimsPrincipal.Identities.OfType<UmbracoBackOfficeIdentity>().FirstOrDefault();
                if (backOfficeIdentity != null) return backOfficeIdentity;
            }

            //Otherwise convert to a UmbracoBackOfficeIdentity if it's auth'd and has the back office session            
            var claimsIdentity = http.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null && claimsIdentity.IsAuthenticated)
            {                
                try
                {
                    return UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity);
                }
                catch (InvalidOperationException ex)
                {
                    //This will occur if the required claim types are missing which would mean something strange is going on
                    LogHelper.Error(typeof(AuthenticationExtensions), "The current identity cannot be converted to " + typeof(UmbracoBackOfficeIdentity), ex);
                }
            }

            if (authenticateRequestIfNotFound == false) return null;

            //even if authenticateRequestIfNotFound is true we cannot continue if the request is actually authenticated 
            // which would mean something strange is going on that it is not an umbraco identity.
            if (http.User.Identity.IsAuthenticated) return null;

            //So the user is not authed but we've been asked to do the auth if authenticateRequestIfNotFound = true,
            // which might occur in old webforms style things or for routes that aren't included as a back office request.
            // in this case, we are just reverting to authing using the cookie.
            
            // TODO: Even though this is in theory legacy, we have legacy bits laying around and we'd need to do the auth based on 
            // how the Module will eventually do it (by calling in to any registered authenticators).
            
            var ticket = http.GetUmbracoAuthTicket();
            if (http.AuthenticateCurrentRequest(ticket, true))
            {
                //now we 'should have an umbraco identity
                return http.User.Identity as UmbracoBackOfficeIdentity;
            }
            return null;
        }

        /// <summary>
        /// This will return the current back office identity. 
        /// </summary>
        /// <param name="http"></param>
        /// <param name="authenticateRequestIfNotFound">
        /// If set to true and a back office identity is not found and not authenticated, this will attempt to authenticate the 
        /// request just as is done in the Umbraco module and then set the current identity if it is valid
        /// </param>
        /// <returns>
        /// Returns the current back office identity if an admin is authenticated otherwise null
        /// </returns>
        internal static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContext http, bool authenticateRequestIfNotFound)
        {
            if (http == null) throw new ArgumentNullException("http");
            return new HttpContextWrapper(http).GetCurrentIdentity(authenticateRequestIfNotFound);
        }

        public static void UmbracoLogout(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");
            Logout(http, UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
        }

        /// <summary>
        /// This clears the forms authentication cookie for webapi since cookies are handled differently
        /// </summary>
        /// <param name="response"></param>
        [Obsolete("Use OWIN IAuthenticationManager.SignOut instead", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void UmbracoLogoutWebApi(this HttpResponseMessage response)
        {
            throw new NotSupportedException("This method is not supported and should not be used, it has been removed in Umbraco 7.4");
        }

        [Obsolete("Use WebSecurity.SetPrincipalForRequest", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static FormsAuthenticationTicket UmbracoLoginWebApi(this HttpResponseMessage response, IUser user)
        {
            throw new NotSupportedException("This method is not supported and should not be used, it has been removed in Umbraco 7.4");            
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        /// <param name="http"></param>
        internal static void UmbracoLogout(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException("http");
            new HttpContextWrapper(http).UmbracoLogout();
        }
        
        /// <summary>
        /// This will force ticket renewal in the OWIN pipeline
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool RenewUmbracoAuthTicket(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");
            http.Items["umbraco-force-auth"] = true;
            return true;
        }

        /// <summary>
        /// This will force ticket renewal in the OWIN pipeline
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        internal static bool RenewUmbracoAuthTicket(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException("http");
            http.Items["umbraco-force-auth"] = true;
            return true;
        }

        /// <summary>
        /// Creates the umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userdata"></param>
        public static FormsAuthenticationTicket CreateUmbracoAuthTicket(this HttpContextBase http, UserData userdata)
        {
            if (http == null) throw new ArgumentNullException("http");
            if (userdata == null) throw new ArgumentNullException("userdata");
            var userDataString = JsonConvert.SerializeObject(userdata);
            return CreateAuthTicketAndCookie(
                http, 
                userdata.Username, 
                userDataString, 
                //use the configuration timeout - this is the same timeout that will be used when renewing the ticket.
                GlobalSettings.TimeOutInMinutes, 
                //Umbraco has always persisted it's original cookie for 1 day so we'll keep it that way
                1440, 
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieDomain);
        }

        internal static FormsAuthenticationTicket CreateUmbracoAuthTicket(this HttpContext http, UserData userdata)
        {
            if (http == null) throw new ArgumentNullException("http");
            if (userdata == null) throw new ArgumentNullException("userdata");
            return new HttpContextWrapper(http).CreateUmbracoAuthTicket(userdata);
        }

        /// <summary>
        /// returns the number of seconds the user has until their auth session times out
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");
            var ticket = http.GetUmbracoAuthTicket();
            return ticket.GetRemainingAuthSeconds();
        }

        /// <summary>
        /// returns the number of seconds the user has until their auth session times out
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                return 0;
            }
            var utcExpired = ticket.Expiration.ToUniversalTime();
            var secondsRemaining = utcExpired.Subtract(DateTime.UtcNow).TotalSeconds;
            return secondsRemaining;
        }

        /// <summary>
        /// Gets the umbraco auth ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContextBase http)
        {
            if (http == null) throw new ArgumentNullException("http");
            return GetAuthTicket(http, UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
        }

        internal static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContext http)
        {
            if (http == null) throw new ArgumentNullException("http");
            return new HttpContextWrapper(http).GetUmbracoAuthTicket();
        }

        internal static FormsAuthenticationTicket GetUmbracoAuthTicket(this IOwinContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException("ctx");
            //get the ticket
            try
            {
                return GetAuthTicket(ctx.Request.Cookies.ToDictionary(x => x.Key, x => x.Value), UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
            }
            catch (Exception)
            {
                //TODO: Do we need to do more here?? need to make sure that the forms cookie is gone, but is that
                // taken care of in our custom middleware somehow?
                ctx.Authentication.SignOut(
                    Core.Constants.Security.BackOfficeAuthenticationType,
                    Core.Constants.Security.BackOfficeExternalAuthenticationType);
                return null;
            }
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        private static void Logout(this HttpContextBase http, string cookieName)
        {
            if (http == null) throw new ArgumentNullException("http");
            //clear the preview cookie and external login
            var cookies = new[] { cookieName, Constants.Web.PreviewCookieName, Constants.Security.BackOfficeExternalCookieName };
            foreach (var c in cookies)
            {
                //remove from the request
                http.Request.Cookies.Remove(c);

                //expire from the response
                var formsCookie = http.Response.Cookies[c];
                if (formsCookie != null)
                {
                    //this will expire immediately and be removed from the browser
                    formsCookie.Expires = DateTime.Now.AddYears(-1);
                }
                else
                {
                    //ensure there's def an expired cookie
                    http.Response.Cookies.Add(new HttpCookie(c) { Expires = DateTime.Now.AddYears(-1) });
                }               
            }
        }

        private static FormsAuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var asDictionary = new Dictionary<string, string>();
            for (var i = 0; i < http.Request.Cookies.Keys.Count; i++)
            {
                var key = http.Request.Cookies.Keys.Get(i);
                asDictionary[key] = http.Request.Cookies[key].Value;
            }

            //get the ticket
            try
            {

                return GetAuthTicket(asDictionary, cookieName);
            }
            catch (Exception)
            {
                //occurs when decryption fails
                http.Logout(cookieName);
                return null;
            }
        }

        private static FormsAuthenticationTicket GetAuthTicket(IDictionary<string, string> cookies, string cookieName)
        {
            if (cookies == null) throw new ArgumentNullException("cookies");

            if (cookies.ContainsKey(cookieName) == false) return null;

            var formsCookie = cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            //get the ticket
            return FormsAuthentication.Decrypt(formsCookie);
        }

        /// <summary>
        /// Creates a custom FormsAuthentication ticket with the data specified
        /// </summary>
        /// <param name="http">The HTTP.</param>
        /// <param name="username">The username.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="loginTimeoutMins">The login timeout mins.</param>
        /// <param name="minutesPersisted">The minutes persisted.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="cookieDomain">The cookie domain.</param>
        private static FormsAuthenticationTicket CreateAuthTicketAndCookie(this HttpContextBase http,
                                            string username,
                                            string userData,
                                            int loginTimeoutMins,
                                            int minutesPersisted,
                                            string cookieName,
                                            string cookieDomain)
        {
            if (http == null) throw new ArgumentNullException("http");
            // Create a new ticket used for authentication
            var ticket = new FormsAuthenticationTicket(
                4,
                username,
                DateTime.Now,
                DateTime.Now.AddMinutes(loginTimeoutMins),
                true,
                userData,
                "/"
                );
	
            // Encrypt the cookie using the machine key for secure transport
            var hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(
                cookieName,
                hash)
                {
                    Expires = DateTime.Now.AddMinutes(minutesPersisted),
                    Domain = cookieDomain,
                    Path = "/"
                };

			if (GlobalSettings.UseSSL)
                cookie.Secure = true;

            //ensure http only, this should only be able to be accessed via the server
            cookie.HttpOnly = true;
				
            http.Response.Cookies.Set(cookie);

            return ticket;
        }
    }
}