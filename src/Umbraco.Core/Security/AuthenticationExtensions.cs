using System;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Security
{
    internal static class FormsAuthenticationTicketExtensions
    {
        public static UmbracoBackOfficeIdentity CreateUmbracoIdentity(this FormsAuthenticationTicket ticket)
        {
            try
            {
                //create the Umbraco user identity 
                return new UmbracoBackOfficeIdentity(ticket);
            }
            catch (Exception ex)
            {
                //This might occur if we cannot decrypt the value in which case we'll just ignore it, it will 
                // be handled by the base pages
                LogHelper.Error(typeof(FormsAuthenticationTicketExtensions), "An error occurred decrypting the user's ticket", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// Extensions to create and renew and remove authentication tickets for the Umbraco back office
    /// </summary>
    internal static class AuthenticationExtensions
    {
        //public static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContextBase http)
        //{
        //    return http.User.Identity as UmbracoBackOfficeIdentity;             
        //}

        //internal static UmbracoBackOfficeIdentity GetCurrentIdentity(this HttpContext http)
        //{
        //    return new HttpContextWrapper(http).GetCurrentIdentity();
        //}
        
        /// <summary>
        /// This clears the authentication cookie
        /// </summary>
        public static void UmbracoLogout(this HttpContextBase http)
        {
            Logout(http, UmbracoSettings.AuthCookieName);
        }

        internal static void UmbracoLogout(this HttpContext http)
        {
            new HttpContextWrapper(http).UmbracoLogout();
        }

        /// <summary>
        /// Creates the umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userdata"></param>
        public static void CreateUmbracoAuthTicket(this HttpContextBase http, UserData userdata)
        {
            CreateAuthTicket(
                http, 
                userdata, 
                //This is one full day... this is how Umbraco has always created this cookie, it is setup to always
                //expire one day from issue and it never gets updated.
                1440, 
                "/", 
                UmbracoSettings.AuthCookieName, 
                UmbracoSettings.AuthCookieDomain);
        }

        internal static void CreateUmbracoAuthTicket(this HttpContext http, UserData userdata)
        {
            new HttpContextWrapper(http).CreateUmbracoAuthTicket(userdata);
        }

        /// <summary>
        /// Gets the umbraco auth ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContextBase http)
        {
            return GetAuthTicket(http, UmbracoSettings.AuthCookieName);
        }

        internal static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContext http)
        {
            return new HttpContextWrapper(http).GetUmbracoAuthTicket();
        }

        /// <summary>
        /// This clears the authentication cookie
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

        /// <summary>
        /// In v6 this is a custom cookie, in v7 this is a real formsauth cookie.
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        private static FormsAuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var formsCookie = http.Request.Cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            
            try
            {
                //get the cookie value
                var cookieVal = formsCookie.Value.DecryptWithMachineKey();

                //here we need to see if the cookie val can be serialized into UserData, if not it means it's probably an old cookie
                var deserialized = JsonConvert.DeserializeObject<UserData>(cookieVal);

                //in v6, we're not using real FormsAuth but our own custom cookie and then we just return a custom FormsAuth ticket
                // for this request.
                return new FormsAuthenticationTicket(
                    4,
                    deserialized.RealName,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(GlobalSettings.TimeOutInMinutes),
                    false,
                    cookieVal,
                    "/");

            }
            catch (Exception)
            {
                //occurs when decryption fails
                http.Logout(cookieName);
                return null;
            }
        }

        /// <summary>
        /// Creates a custom umbraco auth cookie with the data specified
        /// </summary>
        /// <param name="http">The HTTP.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="minutesPersisted">The minutes persisted.</param>
        /// <param name="cookiePath">The cookie path.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="cookieDomain">The cookie domain.</param>
        private static void CreateAuthTicket(this HttpContextBase http,
                                            UserData userData,
                                            int minutesPersisted,
                                            string cookiePath,
                                            string cookieName,
                                            string cookieDomain)
        {
            var cookie = new HttpCookie(cookieName);

            if (GlobalSettings.UseSSL)
                cookie.Secure = true;

            //ensure http only, this should only be able to be accessed via the server
            cookie.HttpOnly = true;
            cookie.Path = cookiePath;
            cookie.Domain = cookieDomain;
            cookie.Expires = DateTime.Now.AddMinutes(minutesPersisted);

            //serialize the user data
            var json = JsonConvert.SerializeObject(userData);
            //encrypt it
            var encTicket = json.EncryptWithMachineKey();

            //set the cookie value
            cookie.Value = encTicket;
                
            http.Response.Cookies.Set(cookie);
        }
    }
}