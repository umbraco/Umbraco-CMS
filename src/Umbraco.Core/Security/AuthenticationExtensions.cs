using System;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Extensions to create and renew and remove authentication tickets for the Umbraco back office
    /// </summary>
    internal static class AuthenticationExtensions
    {
        public static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContextBase http)
        {
            return http.User.Identity as UmbracoBackOfficeIdentity;             
        }

        internal static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContext http)
        {
            return new HttpContextWrapper(http).GetCurrentIdentity();
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        public static void UmbracoLogout(this HttpContextBase http)
        {
            Logout(http, UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
        }

        internal static void UmbracoLogout(this HttpContext http)
        {
            new HttpContextWrapper(http).UmbracoLogout();
        }

        /// <summary>
        /// Renews the Umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool RenewUmbracoAuthTicket(this HttpContextBase http)
        {
            return RenewAuthTicket(http,
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieDomain);
        }

        internal static bool RenewUmbracoAuthTicket(this HttpContext http)
        {
            return new HttpContextWrapper(http).RenewUmbracoAuthTicket();
        }

        /// <summary>
        /// Creates the umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userdata"></param>
        public static void CreateUmbracoAuthTicket(this HttpContextBase http, UserData userdata)
        {
            var userDataString = JsonConvert.SerializeObject(userdata);
            CreateAuthTicket(
                http, 
                userdata.Username, 
                userDataString, 
                //use the configuration timeout - this is the same timeout that will be used when renewing the ticket.
                GlobalSettings.TimeOutInMinutes, 
                //Umbraco has always persisted it's original cookie for 1 day so we'll keep it that way
                1440, 
                "/",
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName,
                UmbracoConfig.For.UmbracoSettings().Security.AuthCookieDomain);
        }

        internal static void CreateUmbracoAuthTicket(this HttpContext http, UserData userdata)
        {
            new HttpContextWrapper(http).CreateUmbracoAuthTicket(userdata);
        }

        /// <summary>
        /// returns the number of seconds the user has until their auth session times out
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this HttpContextBase http)
        {
            var ticket = http.GetUmbracoAuthTicket();
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
            return GetAuthTicket(http, UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName);
        }

        internal static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContext http)
        {
            return new HttpContextWrapper(http).GetUmbracoAuthTicket();
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        private static void Logout(this HttpContextBase http, string cookieName)
        {
            //remove from the request
            http.Request.Cookies.Remove(cookieName);

            //expire from the response
            var formsCookie = http.Response.Cookies[cookieName];
            if (formsCookie != null)
            {
                //this will expire immediately and be removed from the browser
                formsCookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }

        private static FormsAuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var formsCookie = http.Request.Cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            //get the ticket
            try
            {
                return FormsAuthentication.Decrypt(formsCookie.Value);
            }
            catch (Exception)
            {
                //occurs when decryption fails
                http.Logout(cookieName);
                return null;
            }
        }

        /// <summary>
        /// Renews the forms authentication ticket & cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        /// <param name="cookieDomain"></param>
        /// <returns>true if there was a ticket to renew otherwise false if there was no ticket</returns>
        private static bool RenewAuthTicket(this HttpContextBase http, string cookieName, string cookieDomain)
        {
            //get the ticket
            var ticket = GetAuthTicket(http, cookieName);
            //renew the ticket
            var renewed = FormsAuthentication.RenewTicketIfOld(ticket);
            if (renewed == null)
            {
                return false;
            }

            //get the request cookie to get it's expiry date, 
            //NOTE: this will never be null becaues we already do this
            // check in teh GetAuthTicket.
            var formsCookie = http.Request.Cookies[cookieName];
            
            //encrypt it
            var hash = FormsAuthentication.Encrypt(renewed);
            //write it to the response
            var cookie = new HttpCookie(cookieName, hash)
                {
                    Expires = formsCookie.Expires,
                    Domain = cookieDomain
                };
            //rewrite the cooke
            http.Response.Cookies.Set(cookie);
            return true;
        }

        /// <summary>
        /// Creates a custom FormsAuthentication ticket with the data specified
        /// </summary>
        /// <param name="http">The HTTP.</param>
        /// <param name="username">The username.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="loginTimeoutMins">The login timeout mins.</param>
        /// <param name="minutesPersisted">The minutes persisted.</param>
        /// <param name="cookiePath">The cookie path.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="cookieDomain">The cookie domain.</param>
        private static void CreateAuthTicket(this HttpContextBase http,
                                            string username,
                                            string userData,
                                            int loginTimeoutMins,
                                            int minutesPersisted,
                                            string cookiePath,
                                            string cookieName,
                                            string cookieDomain)
        {
            // Create a new ticket used for authentication
            var ticket = new FormsAuthenticationTicket(
                4,
                username,
                DateTime.Now,
                DateTime.Now.AddMinutes(loginTimeoutMins),
                true,
                userData,
                cookiePath
                );
	
            // Encrypt the cookie using the machine key for secure transport
            var hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(
                cookieName,
                hash)
                {
                    Expires = DateTime.Now.AddMinutes(minutesPersisted),
                    Domain = cookieDomain
                };

			if (GlobalSettings.UseSSL)
                cookie.Secure = true;

            //ensure http only, this should only be able to be accessed via the server
            cookie.HttpOnly = true;
				
            http.Response.Cookies.Set(cookie);
        }
    }
}