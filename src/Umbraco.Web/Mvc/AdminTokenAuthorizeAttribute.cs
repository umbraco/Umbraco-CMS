using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used for authorizing scheduled tasks
    /// </summary>
    public sealed class AdminTokenAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly ApplicationContext _applicationContext;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="appContext"></param>
        public AdminTokenAuthorizeAttribute(ApplicationContext appContext)
        {
            if (appContext == null) throw new ArgumentNullException("appContext");
            _applicationContext = appContext;
        }

        public AdminTokenAuthorizeAttribute()
        {
        }

        private ApplicationContext GetApplicationContext()
        {
            return _applicationContext ?? ApplicationContext.Current;
        }

        public const string AuthorizationType = "AToken";

        /// <summary>
        /// Used to return the full value that needs to go in the Authorization header
        /// </summary>
        /// <param name="appContext"></param>
        /// <returns></returns>
        public static string GetAuthHeaderTokenVal(ApplicationContext appContext)
        {
            return string.Format("{0} {1}", AuthorizationType, GetAuthHeaderVal(appContext));
        }

        public static AuthenticationHeaderValue GetAuthenticationHeaderValue(ApplicationContext appContext)
        {
            return new AuthenticationHeaderValue(AuthorizationType, GetAuthHeaderVal(appContext));
        }

        private static string GetAuthHeaderVal(ApplicationContext appContext)
        {
            var admin = appContext.Services.UserService.GetUserById(0);

            var token = string.Format("{0}u____u{1}u____u{2}", admin.Email, admin.Username, admin.RawPasswordValue);

            var encrypted = token.EncryptWithMachineKey();
            var bytes = Encoding.UTF8.GetBytes(encrypted);
            var base64 = Convert.ToBase64String(bytes);
            return string.Format("val=\"{0}\"", base64);
        }
        
        /// <summary>
        /// Ensures that the user must be in the Administrator or the Install role
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");

            var appContext = GetApplicationContext();

            //we need to that the app is configured and that a user is logged in
            if (appContext.IsConfigured == false) return false;

            //need the auth header
            if (httpContext.Request.Headers["Authorization"] == null || httpContext.Request.Headers["Authorization"].IsNullOrWhiteSpace()) return false;

            var header = httpContext.Request.Headers["Authorization"];
            if (header.StartsWith("AToken ") == false) return false;

            var keyVal = Regex.Matches(header, "AToken val=(.*?)(?:$|\\s)");
            if (keyVal.Count != 1) return false;
            if (keyVal[0].Groups.Count != 2) return false;

            var admin = appContext.Services.UserService.GetUserById(0);
            if (admin == null) return false;

            try
            {
                //get the token value from the header
                var val = keyVal[0].Groups[1].Value.Trim("\"");
                //un-base 64 the string
                var bytes = Convert.FromBase64String(val);
                var encrypted = Encoding.UTF8.GetString(bytes);
                //decrypt the string
                var text = encrypted.DecryptWithMachineKey();

                //split - some users have not set an email, don't strip out empty entries
                var split = text.Split(new[] {"u____u"}, StringSplitOptions.None);
                if (split.Length != 3) return false;

                //compare
                return
                    split[0] == admin.Email
                    && split[1] == admin.Username
                    && split[2] == admin.RawPasswordValue;
            }
            catch (Exception ex)
            {
                LogHelper.Error<AdminTokenAuthorizeAttribute>("Failed to format passed in token value", ex);
                return false;
            }
        }
    }
}